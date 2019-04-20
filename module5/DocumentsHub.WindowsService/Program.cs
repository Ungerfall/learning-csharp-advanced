using System;
using System.Diagnostics;
using Messaging.RabbitMQ;
using Topshelf;

namespace DocumentsHub.WindowsService
{
	public class Program
	{
		private const string LOG_FILE = "log.txt";
		private const string SERVICE_NAME = "DocumentsHub";

		private static DocumentsReceiverWorker documentsReceiver;

		static void Main(string[] args)
		{
			InitializeDependencies();
			var rc = HostFactory.Run(x =>
			{
				x.Service<DocumentsReceiverWorker>(s =>
				{
					s.ConstructUsing(
						name => documentsReceiver);
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
			var queue = new ChunkedQueue();
			var path = AppDomain.CurrentDomain.BaseDirectory;
			IDocumentSaver fileSaver = new SimpleFileSaver(path);
			documentsReceiver = new DocumentsReceiverWorker(queue, fileSaver);
		}
	}
}
