using System.Configuration;
using System.Text;
using Messaging.Configuration;
using RabbitMQ.Client;

namespace Messaging.RabbitMQ
{
	public class StatusMessagesConsumer : IMessageConsumer<string>
	{
		private readonly Encoding encoding = Encoding.UTF8;
		private readonly ConnectionFactory factory;
		private const string SERVICE_CONFIGURATION_SECTION = "Messaging";
		private readonly MessagingConfigurationSection configuration
			= (MessagingConfigurationSection) ConfigurationManager.GetSection(SERVICE_CONFIGURATION_SECTION);

		public StatusMessagesConsumer()
		{
			factory = new ConnectionFactory
			{
				HostName = configuration.HostName,
				VirtualHost = "/",
				Port = configuration.Port,
				UserName = configuration.UserName,
				Password = configuration.Password
			};
		}
		public string ReceiveMessage()
		{
			using(var connection = factory.CreateConnection())
			using(var channel = connection.CreateModel())
			{
				EnsureQueueExists(channel);
				var message = channel.BasicGet(configuration.StatusQueue, true);
				if (message == null)
				{
					return null;
				}

				return encoding.GetString(message.Body);
			}
		}

		private void EnsureQueueExists(IModel channel)
		{
			channel.QueueDeclare(
				queue: configuration.StatusQueue,
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null);
		}
	}
}