using System;
using System.Configuration;
using Messaging.Configuration;

namespace Messaging
{
	public abstract class SubscriberBase : IMessageSubscriber
	{
		private const string SERVICE_CONFIGURATION_SECTION = "Messaging";

		protected string HostName;
		protected int Port;
		protected string UserName;
		protected string Password;
		protected string ExchangeName;

		protected SubscriberBase()
		{
			var configuration
				= (MessagingConfigurationSection) ConfigurationManager.GetSection(SERVICE_CONFIGURATION_SECTION);
			HostName = configuration.HostName;
			Port = configuration.Port;
			UserName = configuration.UserName;
			Password = configuration.Password;
			ExchangeName = configuration.CommandsExchange;
		}

		void IMessageSubscriber.Subscribe(Action<byte[]> action)
		{
			Subscribe(action);
		}

		protected abstract void Subscribe(Action<byte[]> action);
	}
}