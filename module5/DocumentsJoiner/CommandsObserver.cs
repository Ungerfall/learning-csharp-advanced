using System;
using System.Threading;
using Documents.Core;
using DocumentsJoiner.Configuration;
using Messaging;
using Messaging.Command;
using Utility.Logging;

namespace DocumentsJoiner
{
	public class CommandsObserver : Worker
	{
		private readonly IMessageSubscriber commandsQueue;
		private readonly DocumentsJoinerStatusObserverWorker statusObserver;

		public CommandsObserver(
			IMessageSubscriber commandsQueue,
			DocumentsJoinerStatusObserverWorker statusObserver)
		{
			this.commandsQueue = commandsQueue ?? throw new ArgumentNullException(nameof(commandsQueue));
			this.statusObserver = statusObserver ?? throw new ArgumentNullException(nameof(statusObserver));
		}

		protected override void OnWorkerStart(CancellationToken token)
		{
			commandsQueue.Subscribe(command =>
			{
				var cmd = CommandMessage<ConfigurationDto>.Deserialize(command);
				SimpleLog.WriteLine($"cmd: {cmd.CommandName}");
				switch (cmd.CommandName)
				{
					case CommandMessageName.UPDATE_CONFIGURATION_COMMAND_NAME:
						var cfg = cmd.Payload;
						ConfigurationContext.Timeout = cfg.Timeout;
						ConfigurationContext.BarcodeSeparator = cfg.BarcodeSeparator;

						break;
					case CommandMessageName.SEND_STATUS_COMMAND_NAME:
						statusObserver.PublishStatusMessages();
						break;
				}
			});

			token.WaitHandle.WaitOne();
		}

		protected override void OnWorkerStop()
		{
			var disposable = commandsQueue as IDisposable;
			disposable?.Dispose();
		}
	}
}