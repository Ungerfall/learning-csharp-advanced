using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DocumentsJoiner.Configuration;
using DocumentsJoiner.Handlers;
using DocumentsJoiner.IO;
using DocumentsJoiner.Wrappers;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace DocumentsJoiner.Tests
{
	[TestFixture]
	public class DocumentsJoinerWorkerTest
	{
		[Test]
		public void CanProcessExistingFilesOnStart()
		{
			var config
				= (DocumentsJoinerConfigurationSection) ConfigurationManager.GetSection("DocumentsJoiner");
			SetupDirectories(config);

			var testFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/test.png");
			var watcherDir = config.Watchers.Cast<FolderWatcherConfigurationElement>().First().Path;
			File.Copy(testFile, Path.Combine(watcherDir, "prefix_001.png"), true);
			File.Copy(testFile, Path.Combine(watcherDir, "prefix_002.png"), true);
			File.Copy(testFile, Path.Combine(watcherDir, "prefix_004.png"), true);
			File.Copy(testFile, Path.Combine(watcherDir, "prefix2_012.png"), true);
			File.Copy(testFile, Path.Combine(watcherDir, "kasjd_aksj.png"), true);
			var worker = CreateWorker(config);
			worker.Start();

			try
			{
				Assert.AreEqual(1, Directory.GetFiles(config.BrokenFilesDirectory).Length);
				Assert.AreEqual(3, Directory.GetFiles(config.BatchesFolder).Length);
			}
			finally
			{
				CleanDirectories(config);
			}
		}

		[Test]
		public async Task CanProcessNewFiles()
		{
			var config
				= (DocumentsJoinerConfigurationSection) ConfigurationManager.GetSection("DocumentsJoiner");
			SetupDirectories(config);

			var worker = CreateWorker(config);
			worker.Start();
			var testFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/test.png");
			var watcherDir = config.Watchers.Cast<FolderWatcherConfigurationElement>().First().Path;
			await Task.Delay(3000);
			File.Copy(testFile, Path.Combine(watcherDir, "prefix_001.png"), true);
			await Task.Delay(3000);
			File.Copy(testFile, Path.Combine(watcherDir, "prefix_002.png"), true);
			await Task.Delay(3000);
			File.Copy(testFile, Path.Combine(watcherDir, "prefix_004.png"), true);
			await Task.Delay(3000);
			File.Copy(testFile, Path.Combine(watcherDir, "prefix2_012.png"), true);
			await Task.Delay(3000);
			File.Copy(testFile, Path.Combine(watcherDir, "kasjd_aksj.png"), true);
			// sleep to wait till file watcher rises an event
			await Task.Delay(60 * 1000);
			worker.Stop();

			try
			{
				Assert.AreEqual(1, Directory.GetFiles(config.BrokenFilesDirectory).Length);
				Assert.AreEqual(3, Directory.GetFiles(config.BatchesFolder).Length);
			}
			finally
			{
				CleanDirectories(config);
			}
		}

		[Test]
		public async Task CanCollectBatchAfterTimeout()
		{
			var config
				= (DocumentsJoinerConfigurationSection) ConfigurationManager.GetSection("DocumentsJoiner");
			SetupDirectories(config);

			var worker = CreateWorker(config);
			worker.Start();
			var testFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/test.png");
			var watcherDir = config.Watchers.Cast<FolderWatcherConfigurationElement>().First().Path;
			await Task.Delay(3000);
			File.Copy(testFile, Path.Combine(watcherDir, "prefix_001.png"), true);
			// wait till timeout
			await Task.Delay(15 * 1000);
			File.Copy(testFile, Path.Combine(watcherDir, "prefix_002.png"), true);
			// sleep to wait till file watcher rises an event
			await Task.Delay(20 * 1000);
			worker.Stop();

			try
			{
				Assert.AreEqual(2, Directory.GetFiles(config.BatchesFolder).Length);
			}
			finally
			{
				CleanDirectories(config);
			}
		}

		[Test]
		public async Task EmptyFileNotCreatedWhenOnlyBrokenFile()
		{
			var config
				= (DocumentsJoinerConfigurationSection) ConfigurationManager.GetSection("DocumentsJoiner");
			SetupDirectories(config);

			var worker = CreateWorker(config);
			worker.Start();
			var testFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/test.png");
			var watcherDir = config.Watchers.Cast<FolderWatcherConfigurationElement>().First().Path;
			await Task.Delay(3000);
			File.Copy(testFile, Path.Combine(watcherDir, "aksldoasd_asldjlsad.png"), true);
			// sleep to wait till file watcher rises an event
			await Task.Delay(20 * 1000);
			worker.Stop();

			try
			{
				Assert.AreEqual(0, Directory.GetFiles(config.BatchesFolder).Length);
				Assert.AreEqual(1, Directory.GetFiles(config.BrokenFilesDirectory).Length);
			}
			finally
			{
				CleanDirectories(config);
			}
		}

		private void CleanDirectories(DocumentsJoinerConfigurationSection config)
		{
			Directory.Delete(config.BrokenFilesDirectory, recursive: true);
			Directory.Delete(config.BatchesFolder, recursive: true);
			foreach (var element in config.Watchers.Cast<FolderWatcherConfigurationElement>())
			{
				Directory.Delete(element.Path, recursive: true);
			}
		}

		private void SetupDirectories(DocumentsJoinerConfigurationSection config)
		{
			Directory.CreateDirectory(config.BrokenFilesDirectory);
			Directory.CreateDirectory(config.BatchesFolder);
			foreach (var element in config.Watchers.Cast<FolderWatcherConfigurationElement>())
			{
				Directory.CreateDirectory(element.Path);
			}
		}

		private DocumentsJoinerWorker CreateWorker(DocumentsJoinerConfigurationSection configuration)
		{
			Trace.Listeners.Add(new TextWriterTraceListener("log.txt"));
			var detector = new ZXingBarcodeDetector();
			Func<DocumentsController, SortedDictionary<int, IDocumentHandler>> handlersChainFactory = ctrl =>
				{
					var sorted = new SortedDictionary<int, IDocumentHandler>
					{
						{1, new DocumentValidator(configuration.BrokenFilesDirectory)},
						{2, new BarCodeHandler(detector, ctrl)},
						{3, new ImageHandler(ctrl)},
					};

					return sorted;
				};
			var fileReader = new WaitForFile(configuration.AttemptsToOpenFile, configuration.OpeningFilePeriodMs);
			var exceptionsHandler = new ErrorHandler(configuration.BrokenFilesDirectory);
			Func<DocumentsController> controllersFactory
				= () => new DocumentsController(
					handlersChainFactory,
					exceptionsHandler,
					fileReader,
					configuration.Timeout);
			Func<CancellationToken, IDocumentsJoiner> joinerFactory
				= token => new PdfSharpDocumentsJoiner(token, fileReader);
			return new DocumentsJoinerWorker(configuration, controllersFactory, joinerFactory);
		}
	}
}