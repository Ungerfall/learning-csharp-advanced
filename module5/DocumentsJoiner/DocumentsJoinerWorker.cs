using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DocumentsJoiner.Configuration;
using Messaging;
using Utility.Logging;

namespace DocumentsJoiner
{
	public class DocumentsJoinerWorker
	{
		private readonly DocumentsJoinerConfigurationSection configuration;
		private readonly Func<DocumentsController> documentsControllerFactory;
		private readonly Func<CancellationToken, IDocumentsJoiner> documentsJoinerFactory;
		private readonly IMessageProducer messageProducer;
		private readonly List<FileSystemWatcher> documentsWatchers = new List<FileSystemWatcher>();
		private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();

		private DocumentsController controller;
		private Thread worker;

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
			this.messageProducer = messageProducer ?? throw new ArgumentNullException(nameof(messageProducer));
			Initialize();
		}

		public void Start()
		{
			ProcessExistingFiles();
			worker = new Thread(
				state =>
				{
					var token = (CancellationToken) state;
					foreach (var watcher in documentsWatchers)
					{
						watcher.EnableRaisingEvents = true;
					}

					WaitHandle.WaitAny(new[] {token.WaitHandle});
					SimpleLog.WriteLine("token cancelled");
				})
			{
				IsBackground = true
			};
			worker.Start(cancellationSource.Token);
		}

		public void Stop()
		{
			cancellationSource.Cancel();
			worker.Join();
			SimpleLog.WriteLine("worker stopped");
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
			SimpleLog.WriteLine($"new document appeared {e.Name}");
			controller.HandleCandidateDocument(e.FullPath);
		}

		private void ControllerOnDocumentsBatchCollected(object sender, DocumentBatchEventArgs e)
		{
			var batch = e.DocumentBatch;
			var joiner = documentsJoinerFactory.Invoke(cancellationSource.Token);
			using (var joinedStream = joiner.Join(batch.Documents))
			using (var ms = new MemoryStream())
			{
				joinedStream.Seek(0, SeekOrigin.Begin);
				joinedStream.CopyTo(ms);
				var message = ms.ToArray();
				messageProducer.SendMessage(message);
			}
		}

		private void ProcessExistingFiles()
		{
			var files = documentsWatchers
				.SelectMany(x => Directory.EnumerateFiles(x.Path, x.Filter))
				.OrderBy(x => x);
			foreach (var file in files)
			{
				controller.HandleCandidateDocument(file);
			}

			controller.CollectBatch();
		}
	}
}