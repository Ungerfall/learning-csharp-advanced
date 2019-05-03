using System.Configuration;
using Messaging.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Messaging.RabbitMQ
{
	public class StatusMessagesProducer : IMessageProducer
	{
		private const string SERVICE_CONFIGURATION_SECTION = "Messaging";
		private const string STATUS_MESSAGE_EXPIRATION_MS = "10000";

		private readonly ConnectionFactory factory;
		private readonly MessagingConfigurationSection configuration
			= (MessagingConfigurationSection) ConfigurationManager.GetSection(SERVICE_CONFIGURATION_SECTION);

		public StatusMessagesProducer()
		{
			factory = new ConnectionFactory()
			{
				HostName = configuration.HostName,
				VirtualHost = "/",
				Port = configuration.Port,
				UserName = configuration.UserName,
				Password = configuration.Password
			};
		}

		public void SendMessage(byte[] message)
		{
			using(var connection = factory.CreateConnection())
			using(var channel = connection.CreateModel())
			{
				EnsureQueueExists(channel);
				IBasicProperties props = new BasicProperties();
				props.Expiration = STATUS_MESSAGE_EXPIRATION_MS;
				channel.BasicPublish(
					exchange: "",
					routingKey: configuration.StatusQueue,
					basicProperties: props,
					body: message);
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