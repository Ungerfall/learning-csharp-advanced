using Messenger.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Messenger.Client
{
	public class MessegesBroker : IDisposable
	{
		private Dictionary<string, IMessengerClient> subscribers = new Dictionary<string, IMessengerClient>();
		private Queue<Message> messagesQueue = new Queue<Message>();
		private NamedPipeClientStream client;
		private StreamReader reader;
		private StreamWriter writer;

		private readonly TextWriter translator;

		public MessegesBroker(TextWriter translator)
		{
			this.translator = translator ?? throw new ArgumentNullException(nameof(translator));
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
			translator.WriteLine("connected to server");
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

		public void EnqueueMessage(string clientName, string message, MessageCode code)
		{
			messagesQueue.Enqueue(new Message { From = clientName, Body = message, Code = code });
		}

		public void Subscribe(IMessengerClient client)
		{
			if (!subscribers.ContainsKey(client.Name))
			{
				subscribers.Add(client.Name, client);
			}

			EnqueueMessage(client.Name, string.Empty, MessageCode.Connect);
		}

		public void Unsubscribe(IMessengerClient client)
		{
			EnqueueMessage(client.Name, string.Empty, MessageCode.Disconnect);
		}

		private void ListenServer(CancellationToken cancellation)
		{
			while (true)
			{
				cancellation.ThrowIfCancellationRequested();
				var serverMessage = reader.ReadLine();
				var message = Message.Deserialize(serverMessage);
				IMessengerClient subscriber;
				if (subscribers.TryGetValue(message.To, out subscriber))
				{
					subscriber.ReceiveMessageFromServer(
						$"{message.From}: {message.Body} for {message.To}");
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

		public bool IsConnected => client.IsConnected;

		public void Dispose()
		{
			try
			{
				writer?.Dispose();
			}
			catch { }
			finally
			{
				writer = null;
			}
			try
			{
				reader?.Dispose();
			}
			catch { }
			finally
			{
				reader = null;
			}
			try
			{
				client?.Dispose();
			}
			catch { }
			finally
			{
				client = null;
			}
		}
	}
}
