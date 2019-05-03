using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Messaging.RabbitMQ
{
	public class CommandsSubscriber : SubscriberBase, IDisposable
	{
		private readonly ConnectionFactory factory;
		private readonly IConnection connection;
		private readonly IModel channel;

		public CommandsSubscriber()
		{
			factory = new ConnectionFactory
			{
				HostName = base.HostName,
				VirtualHost = "/",
				Port = base.Port,
				UserName = base.UserName,
				Password = base.Password
			};
			connection = factory.CreateConnection();
			channel = connection.CreateModel();
		}

		protected override void Subscribe(Action<byte[]> action)
		{
			channel.ExchangeDeclare(ExchangeName, type: "fanout");
			var queueName = channel.QueueDeclare().QueueName;
			channel.QueueBind(queueName, ExchangeName, routingKey: "");
			var consumer = new EventingBasicConsumer(channel);
			consumer.Received += (model, ea) =>
			{
				var body = ea.Body;
				action(body);
			};

			channel.BasicConsume(queueName, autoAck: true, consumer: consumer);
		}

		public void Dispose()
		{
			connection?.Dispose();
			channel?.Dispose();
		}
	}
}