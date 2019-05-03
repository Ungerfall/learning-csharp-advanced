using System;
using System.Threading;
using Documents.Core;
using Messaging;
using Messaging.Command;

namespace DocumentsHub
{
	public class CommandsInvokerWorker : Worker
	{
		private const int SEND_STATUS_COMMAND_PERIOD_MS = 10 * 1000;

		private readonly IMessageProducer commandsQueue;
		private readonly Func<IConfigurationObserver> configurationObserverFactory;

		public CommandsInvokerWorker(
			IMessageProducer commandsQueue,
			Func<IConfigurationObserver> configurationObserverFactory)
		{
			this.commandsQueue = commandsQueue ?? throw new ArgumentNullException(nameof(commandsQueue));
			this.configurationObserverFactory = configurationObserverFactory
				?? throw new ArgumentNullException(nameof(configurationObserverFactory));
		}

		protected override void OnWorkerStart(CancellationToken token)
		{
			IConfigurationObserver obs = null;
			try
			{
				obs = configurationObserverFactory.Invoke();
				obs.PropertyChanged += (o, ea) =>
				{
					var cfgDto = new ConfigurationDto
					{
						Timeout = obs.DocumentsJoinerConfiguration.Timeout,
						BarcodeSeparator = obs.DocumentsJoinerConfiguration.BarcodeSeparatorValue
					};
					var cmd = new CommandMessage<ConfigurationDto>
					{
						CommandName = CommandMessageName.UPDATE_CONFIGURATION_COMMAND_NAME,
						Payload = cfgDto
					};
					commandsQueue.SendMessage(cmd.Serialize());
				};
				obs.Start();
				while (!token.IsCancellationRequested)
				{
					token.WaitHandle.WaitOne(SEND_STATUS_COMMAND_PERIOD_MS);
					if (token.IsCancellationRequested)
					{
						break;
					}

					var statusCmd = new CommandMessage<object>
					{
						CommandName = CommandMessageName.SEND_STATUS_COMMAND_NAME
					};
					commandsQueue.SendMessage(statusCmd.Serialize());
				}
			}
			finally
			{
				obs?.Dispose();
			}
		}

		protected override void OnWorkerStop()
		{
		}
	}
}