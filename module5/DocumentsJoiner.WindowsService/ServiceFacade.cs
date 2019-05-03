using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using DocumentsJoiner.Configuration;
using DocumentsJoiner.Handlers;
using DocumentsJoiner.IO;
using DocumentsJoiner.Wrappers;
using Messaging.RabbitMQ;
using Topshelf;

namespace DocumentsJoiner.WindowsService
{
	public class ServiceFacade : ServiceControl
	{
		private readonly DocumentsJoinerWorker documentsJoinerWorker;
		private readonly DocumentsJoinerStatusObserverWorker documentsJoinerStatusObserverWorker;
		private readonly CommandsObserver commandsObserver;

		public ServiceFacade()
		{
			var configuration = (DocumentsJoinerConfigurationSection) ConfigurationManager
				.GetSection(ConfigurationContext.SERVICE_CONFIGURATION_SECTION);
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
			Func<DocumentsController> controllersFactory =
				() => new DocumentsController(handlersChainFactory, exceptionsHandler, fileReader);
			Func<CancellationToken, IDocumentsJoiner> joinerFactory
				= token => new PdfSharpDocumentsJoiner(token, fileReader);
			var queue = new ChunkedQueue();
			documentsJoinerWorker = new DocumentsJoinerWorker(
				configuration,
				controllersFactory,
				joinerFactory,
				queue);

			var observables = new[] {documentsJoinerWorker};
			var statusQueue = new StatusMessagesProducer();
			documentsJoinerStatusObserverWorker = new DocumentsJoinerStatusObserverWorker(observables, statusQueue);

			var commandsQueue = new CommandsSubscriber();
			commandsObserver = new CommandsObserver(commandsQueue, documentsJoinerStatusObserverWorker);
		}

		public bool Start(HostControl hostControl)
		{
			documentsJoinerWorker.Start();
			documentsJoinerStatusObserverWorker.Start();
			commandsObserver.Start();
			return true;
		}

		public bool Stop(HostControl hostControl)
		{
			documentsJoinerWorker.Stop();
			documentsJoinerStatusObserverWorker.Stop();
			commandsObserver.Stop();
			return true;
		}
	}
}