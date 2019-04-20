using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Utility.Logging;

namespace Messaging.RabbitMQ
{
	public class ChunkedQueue : ChunkedMessageQueueBase
	{
		private ConnectionFactory factory;

		public ChunkedQueue()
		{
			factory = new ConnectionFactory
			{
				HostName = HostName,
				VirtualHost = "/",
				Port = Port,
				UserName = UserName,
				Password = Password
			};
		}

		public override void SendChunkMessage(Chunk chunk)
		{
			using(var connection = factory.CreateConnection())
			using(var channel = connection.CreateModel())
			{
				EnsureQueueExists(channel);
				var props = new BasicProperties
				{
					Headers = new Dictionary<string, object>
					{
						[nameof(Chunk.Guid)] = chunk.Guid.ToString(),
						[nameof(Chunk.IsLast)] = chunk.IsLast,
						[nameof(Chunk.Position)] = chunk.Position,
						[nameof(Chunk.Size)] = chunk.Size
					}
				};
				channel.BasicPublish(
					exchange: "",
					routingKey: DataQueueName,
					basicProperties: props,
					body: chunk.Body);
			}
		}

		public override Chunk ReceiveChunkMessage()
		{
			try
			{
				using(var connection = factory.CreateConnection())
				using(var channel = connection.CreateModel())
				{
					EnsureQueueExists(channel);
					var message = channel.BasicGet(DataQueueName, true);
					if (message == null)
					{
						return Chunk.NullChunk;
					} 

					var body = message.Body;
					var props = message.BasicProperties;
					var guid = Guid.Parse(Encoding.GetString((byte[]) props.Headers[nameof(Chunk.Guid)]));
					var isLast = (bool) props.Headers[nameof(Chunk.IsLast)];
					var pos = (int) props.Headers[nameof(Chunk.Position)];
					var size = (int) props.Headers[nameof(Chunk.Size)];
					var chunk = new Chunk(guid, pos, size, body, isLast);

					return chunk;
				}
			}
			catch (Exception e)
			{
				SimpleLog.WriteLine($"rabbitmq message receive exception: {e.Message}");
				return Chunk.NullChunk;
			}
		}

		private void EnsureQueueExists(IModel channel)
		{
			channel.QueueDeclare(
				queue: DataQueueName,
				durable: false,
				exclusive: false,
				autoDelete: false,
				arguments: null);
		}
	}
}
