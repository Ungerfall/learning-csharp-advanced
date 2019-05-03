using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Documents.Core;
using DocumentsJoiner.Configuration;
using Messaging;
using Utility.Logging;

namespace DocumentsJoiner
{
	public class DocumentsJoinerWorker : Worker, IStatusObservable
	{
		private readonly DocumentsJoinerConfigurationSection configuration;
		private readonly Func<DocumentsController> documentsControllerFactory;
		private readonly Func<CancellationToken, IDocumentsJoiner> documentsJoinerFactory;
		private readonly IMessageProducer documentsQueue;
		private readonly List<FileSystemWatcher> documentsWatchers = new List<FileSystemWatcher>();

		private DocumentsController controller;

		public DocumentsJoinerWorker(
			DocumentsJoinerConfigurationSection configuration,
			Func<DocumentsController> documentsControllerFactory,
			Func<CancellationToken, IDocumentsJoiner> documentsJoinerFactory,
			IMessageProducer messageProducer)
		{
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.documentsControllerFactory
				= documentsControllerFactory ?? throw new ArgumentNullException(nameof(documentsControllerFactory));
			this.documentsJoinerFactory
				= documentsJoinerFactory ?? throw new ArgumentNullException(nameof(documentsJoinerFactory));
			this.documentsQueue = messageProducer ?? throw new ArgumentNullException(nameof(messageProducer));
			Initialize();
		}

		protected override void OnWorkerStart(CancellationToken token)
		{
			ProcessExistingFiles();
			Status = DocumentsJoinerStatus.WAITING;
			foreach (var watcher in documentsWatchers)
			{
				watcher.EnableRaisingEvents = true;
			}

			WaitHandle.WaitAny(new[] {token.WaitHandle});
		}

		protected override void OnWorkerStop()
		{
			Status = DocumentsJoinerStatus.STOPPED;
		}

		private void Initialize()
		{
			// controller
			controller = documentsControllerFactory.Invoke();
			controller.DocumentsBatchCollected += ControllerOnDocumentsBatchCollected;

			// watchers
			foreach (var watcher in configuration.Watchers.Cast<FolderWatcherConfigurationElement>())
			{
				var fileWatcher = new FileSystemWatcher(watcher.Path)
				{
					IncludeSubdirectories = false,
					Filter = watcher.Filter,
				};
				fileWatcher.Created += HandleNewDocument;
				documentsWatchers.Add(fileWatcher);
			}
		}

		private void HandleNewDocument(object sender, FileSystemEventArgs e)
		{
			Status = DocumentsJoinerStatus.PROCESSING_FILES;
			SimpleLog.WriteLine($"new document appeared {e.Name}");
			controller.HandleCandidateDocument(e.FullPath);
			Status = DocumentsJoinerStatus.WAITING;
		}

		private void ControllerOnDocumentsBatchCollected(object sender, DocumentBatchEventArgs e)
		{
			Status = DocumentsJoinerStatus.BUILDING_BATCH;
			var batch = e.DocumentBatch;
			var joiner = documentsJoinerFactory.Invoke(CancellationSource.Token);
			using (var joinedStream = joiner.Join(batch.Documents))
			using (var ms = new MemoryStream())
			{
				joinedStream.Seek(0, SeekOrigin.Begin);
				joinedStream.CopyTo(ms);
				var message = ms.ToArray();
				documentsQueue.SendMessage(message);
			}

			Status = DocumentsJoinerStatus.WAITING;
		}

		private void ProcessExistingFiles()
		{
			Status = DocumentsJoinerStatus.PROCESSING_EXISTING_FILES;
			var files = documentsWatchers
				.SelectMany(x => Directory.EnumerateFiles(x.Path, x.Filter))
				.OrderBy(x => x);
			foreach (var file in files)
			{
				controller.HandleCandidateDocument(file);
			}

			controller.CollectBatch();
		}

		public string Status { get; private set; } = DocumentsJoinerStatus.CREATED;
	}
}