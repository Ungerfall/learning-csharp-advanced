using RabbitMQ.Client;

namespace Messaging.RabbitMQ
{
	public class CommandsPublisher : PublisherBase
	{
		private readonly ConnectionFactory factory;

		public CommandsPublisher()
		{
			factory = new ConnectionFactory
			{
				HostName = base.HostName,
				VirtualHost = "/",
				Port = base.Port,
				UserName = base.UserName,
				Password = base.Password
			};
		}

		protected override void Publish(byte[] message)
		{
			using (var connection = factory.CreateConnection())
			using (var channel = connection.CreateModel())
			{
				channel.ExchangeDeclare(exchange: base.ExchangeName, type: "fanout");

				channel.BasicPublish(
					exchange: base.ExchangeName,
					routingKey: "",
					basicProperties: null,
					body: message);
			}
		}
	}
}