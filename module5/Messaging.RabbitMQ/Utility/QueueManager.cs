using System.Configuration;
using Messaging.Configuration;
using RabbitMQ.Client;

namespace Messaging.RabbitMQ.Utility
{
	public static class QueueManager
	{
		public static void DeleteStatusQueue()
		{
			const string SERVICE_CONFIGURATION_SECTION = "Messaging";
			MessagingConfigurationSection configuration
				= (MessagingConfigurationSection) ConfigurationManager.GetSection(SERVICE_CONFIGURATION_SECTION);

			var factory = new ConnectionFactory
			{
				HostName = configuration.HostName,
				VirtualHost = "/",
				Port = configuration.Port,
				UserName = configuration.UserName,
				Password = configuration.Password
			};
			using(var connection = factory.CreateConnection())
			using(var channel = connection.CreateModel())
			{
				channel.QueueDeleteNoWait(configuration.StatusQueue, ifUnused: false, ifEmpty: false);
			}
		}
	}
}