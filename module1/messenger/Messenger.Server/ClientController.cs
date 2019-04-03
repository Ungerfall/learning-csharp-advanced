using Messenger.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Messenger.Server
{
	public class ClientController : IDisposable
	{
		private CancellationTokenSource cancellationSource = new CancellationTokenSource();
		private Queue<Message> messages = new Queue<Message>();
		private Thread outgoingMessagesWorker;
		private ManualResetEvent onlineEvent = new ManualResetEvent(false);

		private readonly string clientKey;
		private readonly MessengerServer server;

		public ClientController(string clientKey, MessengerServer server)
		{
			this.clientKey = clientKey;
			this.server = server ?? throw new ArgumentNullException(nameof(server));
			outgoingMessagesWorker = new Thread(ProcessMessages)
			{
				IsBackground = true
			};
			var cancellation = cancellationSource.Token;
			outgoingMessagesWorker.Start(cancellation);
		}

		public void EnqueueMessage(Message message)
		{
			messages.Enqueue(message);
		}

		public void Connect()
		{
			Status = ClientStatus.Online;
			onlineEvent.Set();
		}

		public void Disconnect()
		{
			Status = ClientStatus.Offline;
			onlineEvent.Reset();
		}

		public Task Stop()
		{
			cancellationSource.Cancel();
			outgoingMessagesWorker.Join();
			return Task.Run(() => { SendRemainingMessages(); });
		}

		public void Dispose()
		{
			cancellationSource?.Cancel();
			cancellationSource?.Dispose();
		}

		public ClientStatus Status { get; private set; }

		public int MessagesInQueue => messages.Count;

		public ThreadState ThreadState => outgoingMessagesWorker.ThreadState;

		private void ProcessMessages(object state)
		{
			var token = (CancellationToken) state;
			while (true)
			{
				WaitHandle.WaitAny(new []{ onlineEvent, token.WaitHandle });
				if (token.IsCancellationRequested)
					break;

				if (messages.Count == 0) continue;

				var message = messages.Dequeue();
				server.SendMessage(message.Serialize());
			}
		}

		private void SendRemainingMessages()
		{
			while (Status == ClientStatus.Online && messages.Count > 0)
			{
				var message = messages.Dequeue();
				server.SendMessage(message.Serialize());
			}
		}
	}

	public enum ClientStatus
	{
		None = 0,
		Online = 1,
		Offline = 2
	}
}
