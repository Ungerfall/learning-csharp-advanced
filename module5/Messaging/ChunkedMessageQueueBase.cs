using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading;
using Messaging.Configuration;

namespace Messaging
{
	public abstract class ChunkedMessageQueueBase : IMessageProducer, IMessageConsumer<DocumentMessage>
	{
		private const string SERVICE_CONFIGURATION_SECTION = "Messaging";
		private const int RECEIVE_MESSAGE_SLEEP_MS = 500;

		private readonly int chunkSize;
		private readonly Dictionary<Guid, List<Chunk>> messages = new Dictionary<Guid, List<Chunk>>();
		private readonly ChunkedMessageBuilder chunksBuilder = new ChunkedMessageBuilder();
		private readonly CancellationToken token;

		protected string HostName;
		protected int Port;
		protected string UserName;
		protected string Password;
		protected string DataQueueName;
		protected Encoding Encoding = Encoding.UTF8;

		protected ChunkedMessageQueueBase(CancellationToken token)
		{
			var configuration
				= (MessagingConfigurationSection) ConfigurationManager.GetSection(SERVICE_CONFIGURATION_SECTION);
			chunkSize = configuration.ChunkSize;
			HostName = configuration.HostName;
			Port = configuration.Port;
			UserName = configuration.UserName;
			Password = configuration.Password;
			DataQueueName = configuration.DataQueue;
			this.token = token;
		}

		public void SendMessage(byte[] message)
		{
			foreach (var chunk in chunksBuilder.Shred(message, chunkSize))
			{
				SendChunkMessage(chunk);
			}
		}

		public DocumentMessage ReceiveMessage()
		{
			var chunks = new List<Chunk>();
			Chunk chunk = null;
			do
			{
				if (token.IsCancellationRequested)
					break;

				chunk = ReceiveChunkMessage();
				if (chunk.Equals(Chunk.NullChunk))
				{
					Thread.Sleep(RECEIVE_MESSAGE_SLEEP_MS);
					continue;
				}

				chunks.Add(chunk);

			} while (!chunk.IsLast);

			return new DocumentMessage
			{
				Content = chunksBuilder.Build(chunks),
				Name = chunk.Guid.ToString()
			};
		}

		public abstract void SendChunkMessage(Chunk chunk);

		public abstract Chunk ReceiveChunkMessage();

		private List<Chunk> GetBuiltMessage()
		{
			throw new NotImplementedException();
		}
	}
}