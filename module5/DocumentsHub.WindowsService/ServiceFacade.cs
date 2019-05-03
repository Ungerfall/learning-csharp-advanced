using System;
using System.Threading;
using Messaging.RabbitMQ;
using Topshelf;

namespace DocumentsHub.WindowsService
{
	public class ServiceFacade : ServiceControl
	{
		private readonly DocumentsReceiverWorker receiverWorker;
		private readonly CommandsInvokerWorker commandsInvokerWorker;
		private readonly DocumentsJoinerClientStatusReceiverWorker documentsJoinerClientStatusReceiverWorker;

		public ServiceFacade()
		{
			Func<CancellationToken, ChunkedQueue> queueFactory= token => new ChunkedQueue(token);
			var path = AppDomain.CurrentDomain.BaseDirectory;
			IDocumentSaver fileSaver = new SimpleFileSaver(path);
			receiverWorker = new DocumentsReceiverWorker(queueFactory, fileSaver);

			var commandsQueue = new CommandsPublisher();
			Func<IConfigurationObserver> configurationObserverFactory = () => new ConfigurationObserver();
			commandsInvokerWorker = new CommandsInvokerWorker(commandsQueue, configurationObserverFactory);

			var statusesQueue = new StatusMessagesConsumer();
			documentsJoinerClientStatusReceiverWorker = new DocumentsJoinerClientStatusReceiverWorker(statusesQueue);
		}

		public bool Start(HostControl hostControl)
		{
			receiverWorker.Start();
			commandsInvokerWorker.Start();
			documentsJoinerClientStatusReceiverWorker.Start();
			return true;
		}

		public bool Stop(HostControl hostControl)
		{
			receiverWorker.Stop();
			commandsInvokerWorker.Stop();
			documentsJoinerClientStatusReceiverWorker.Stop();
			return true;
		}
	}
}