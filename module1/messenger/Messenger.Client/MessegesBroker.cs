using Messenger.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Messenger.Client
{
	public class MessegesBroker : IDisposable
	{
		private static readonly object subscribersLock = new object();

		private List<IMessengerClient> subscribers = new List<IMessengerClient>();
		private Queue<Message> messagesQueue = new Queue<Message>();

		private readonly NamedPipeClientStream client;
		private readonly StreamReader reader;
		private readonly StreamWriter writer;

		public MessegesBroker()
		{
			client = new NamedPipeClientStream(
				".",
				Configuration.MESSENGER_PIPE,
				PipeDirection.InOut,
				PipeOptions.Asynchronous); 
			reader = new StreamReader(client);
			writer = new StreamWriter(client);
		}

		public void Start()
		{
			var cancellation = default(CancellationToken);
			client.Connect();
			Task.Run(
				() =>
				{
					ListenServer(cancellation);
				},
				cancellation);
			Task.Run(
				() =>
				{
					SendMessagesToServer(cancellation);
				},
				cancellation);
		}

		public void EnqueueMessage(string clientName, string message)
		{
			messagesQueue.Enqueue(new Message { ClientName = clientName, Body = message });
		}

		public void Subscribe(IMessengerClient client)
		{
			lock (subscribersLock)
			{
				if (!subscribers.Contains(client))
				{
					subscribers.Add(client);
				}
			}
		}

		public void Unsubscribe(IMessengerClient messengerClient)
		{
			lock (subscribersLock)
			{
				subscribers.Remove(messengerClient);
			}
		}

		private void ListenServer(CancellationToken cancellation)
		{
			while (true)
			{
				cancellation.ThrowIfCancellationRequested();
				var serverMessage = reader.ReadLine();
				var message = Message.Deserialize(serverMessage);
				lock (subscribersLock)
				{
					foreach (var subscriber in subscribers)
					{
						subscriber.ReceiveMessageFromServer(
							$"For {subscriber.Name} From {message.ClientName}: {message.Body}");
					}
				}
			}
		}

		private void SendMessagesToServer(CancellationToken cancellation)
		{
			while (true)
			{
				cancellation.ThrowIfCancellationRequested();
				if (messagesQueue.Count != 0)
				{
					var message = messagesQueue.Dequeue().Serialize();
					writer.WriteLine(message);
					writer.Flush();
				}
			}
		}

		public void Dispose()
		{
			writer?.Dispose();
			reader?.Dispose();
			client?.Dispose();
		}
	}
}
