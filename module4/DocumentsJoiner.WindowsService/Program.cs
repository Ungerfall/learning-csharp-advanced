using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DocumentsJoiner.Configuration;
using DocumentsJoiner.Handlers;
using DocumentsJoiner.IO;
using DocumentsJoiner.Wrappers;
using Topshelf;

namespace DocumentsJoiner.WindowsService
{
	class Program
	{
		static void Main(string[] args)
		{
			Trace.Listeners.Add(new TextWriterTraceListener("log.txt"));
			var configuration
				= (DocumentsJoinerConfigurationSection) ConfigurationManager.GetSection("DocumentsJoiner");
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
			var rc = HostFactory.Run(x =>
			{
				x.Service<DocumentsJoinerWorker>(s =>
				{
					s.ConstructUsing(
						name=> new DocumentsJoinerWorker(configuration, controllersFactory, joinerFactory));
					s.WhenStarted(tc => tc.Start());
					s.WhenStopped(tc => tc.Stop());
				});
				x.RunAsLocalSystem();

				x.SetDisplayName("DocumentsJoiner");
				x.SetServiceName("DocumentsJoiner");
			});

			var exitCode = (int) Convert.ChangeType(rc, rc.GetTypeCode());
			Environment.ExitCode = exitCode;
		}
	}
}
