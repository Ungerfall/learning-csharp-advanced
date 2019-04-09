using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DocumentsJoiner.Configuration;
using DocumentsJoiner.Utility;

namespace DocumentsJoiner
{
	public class DocumentsJoinerWorker
	{
		private const string PDF_NAME_FORMAT = "{0}_{1}_{2}.pdf";

		private readonly DocumentsJoinerConfigurationSection configuration;
		private readonly Func<DocumentsController> documentsControllerFactory;
		private readonly Func<CancellationToken, IDocumentsJoiner> documentsJoinerFactory;
		private readonly List<FileSystemWatcher> documentsWatchers = new List<FileSystemWatcher>();
		private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();

		private DocumentsController controller;
		private Thread worker;

		public DocumentsJoinerWorker(
			DocumentsJoinerConfigurationSection configuration,
			Func<DocumentsController> documentsControllerFactory,
			Func<CancellationToken, IDocumentsJoiner> documentsJoinerFactory)
		{
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this.documentsControllerFactory
				= documentsControllerFactory ?? throw new ArgumentNullException(nameof(documentsControllerFactory));
			this.documentsJoinerFactory = documentsJoinerFactory ?? throw new ArgumentNullException(nameof(documentsJoinerFactory));
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
			var path = Path.Combine(
				configuration.BatchesFolder,
				string.Format(PDF_NAME_FORMAT, batch.Prefix, batch.MinSequenceNumber, batch.MaxSequenceNumber));
			var joiner = documentsJoinerFactory.Invoke(cancellationSource.Token);
			using (var file = File.Create(path))
			using (var joinedStream = joiner.Join(batch.Documents))
			{
				joinedStream.CopyTo(file);
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