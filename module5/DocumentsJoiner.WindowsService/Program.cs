using System;
using System.Diagnostics;
using Topshelf;
using Topshelf.Logging;

namespace DocumentsJoiner.WindowsService
{
	class Program
	{
		private const string LOG_FILE = "log.txt";
		private const string SERVICE_NAME = "DocumentsJoiner";

		static void Main(string[] args)
		{
			InitializeDependencies();
			var rc = HostFactory.Run(x =>
			{
				x.Service<ServiceFacade>();
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
			Trace.Listeners.Add(new TopshelfConsoleTraceListener());
		}
	}
}
