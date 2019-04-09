using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using DocumentsJoiner.Configuration;
using DocumentsJoiner.Handlers;
using DocumentsJoiner.IO;
using DocumentsJoiner.Wrappers;
using Topshelf;

namespace DocumentsJoiner.WindowsService
{
	class Program
	{
		private const string LOG_FILE = "log.txt";
		private const string SERVICE_NAME = "DocumentsJoiner";
		private const string SERVICE_CONFIGURATION_SECTION = "DocumentsJoiner";

		private static DocumentsJoinerWorker documentsJoinerWorker;

		static void Main(string[] args)
		{
			InitializeDependencies();
			var rc = HostFactory.Run(x =>
			{
				x.Service<DocumentsJoinerWorker>(s =>
				{
					s.ConstructUsing(
						name => documentsJoinerWorker);
					s.WhenStarted(tc => tc.Start());
					s.WhenStopped(tc => tc.Stop());
				});
				x.RunAsLocalSystem();

				x.SetDisplayName(SERVICE_NAME);
				x.SetServiceName(SERVICE_NAME);
			});

			var exitCode = (int) Convert.ChangeType(rc, rc.GetTypeCode());
			Environment.ExitCode = exitCode;
		}

		private static void InitializeDependencies()
		{
			Trace.Listeners.Add(new TextWriterTraceListener(LOG_FILE));
			var configuration
				= (DocumentsJoinerConfigurationSection) ConfigurationManager.GetSection(SERVICE_CONFIGURATION_SECTION);
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
			var fileReader = new WaitForFile(configuration.AttemptsToOpenFile);
			var exceptionsHandler = new ErrorHandler(configuration.BrokenFilesDirectory);
			Func<DocumentsController> controllersFactory
				= () => new DocumentsController(
					handlersChainFactory,
					exceptionsHandler,
					fileReader,
					configuration.Timeout);
			Func<CancellationToken, IDocumentsJoiner> joinerFactory
				= token => new PdfSharpDocumentsJoiner(token, fileReader);
			documentsJoinerWorker = new DocumentsJoinerWorker(configuration, controllersFactory, joinerFactory);
		}
	}
}
