using System.Configuration;
using System.Text;
using Messaging.Configuration;

namespace Messaging
{
	public abstract class PublisherBase : IMessageProducer
	{
		private const string SERVICE_CONFIGURATION_SECTION = "Messaging";

		protected string HostName;
		protected int Port;
		protected string UserName;
		protected string Password;
		protected string ExchangeName;

		protected PublisherBase()
		{
			var configuration
				= (MessagingConfigurationSection) ConfigurationManager.GetSection(SERVICE_CONFIGURATION_SECTION);
			HostName = configuration.HostName;
			Port = configuration.Port;
			UserName = configuration.UserName;
			Password = configuration.Password;
			ExchangeName = configuration.CommandsExchange;
		}

		public void SendMessage(byte[] message)
		{
			Publish(message);
		}

		protected abstract void Publish(byte[] message);
	}
}