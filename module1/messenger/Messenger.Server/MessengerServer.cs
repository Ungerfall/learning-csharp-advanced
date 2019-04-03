using Messenger.Common;
using Messenger.Common.Data;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Messenger.Server
{
	public class MessengerServer : IMessengerServer, IDisposable
	{
		private NamedPipeServerStream server;
		private StreamReader reader;
		private StreamWriter writer;

		private Dictionary<string, ClientController> clientContollers = new Dictionary<string, ClientController>();
		private ConcurrentBag<Message> history = new ConcurrentBag<Message>();

		private CancellationTokenSource messagesCancelltationSource;
		private CancellationTokenSource serverHealthCancellationSource;
		private Thread healthMonitor;
		private Thread messagesDispatcher;

		private readonly IMessagesRepository repository;
		private readonly TextWriter translator;

		public MessengerServer(IMessagesRepository repository, TextWriter translator)
		{
			this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
			this.translator = translator ?? throw new ArgumentNullException(nameof(translator));
			WriteServerConfiguration();
		}

		public async Task StartAsync()
		{
			server = new NamedPipeServerStream(
				Configuration.MESSENGER_PIPE,
				PipeDirection.InOut,
				1,
				PipeTransmissionMode.Byte,
				PipeOptions.Asynchronous);
			reader = new StreamReader(server);
			writer = new StreamWriter(server);
			messagesCancelltationSource = new CancellationTokenSource();
			serverHealthCancellationSource = new CancellationTokenSource();
			var cancellation = messagesCancelltationSource.Token;

			await server.WaitForConnectionAsync(cancellation);
			StartServerHealthMonitor(serverHealthCancellationSource.Token);
			await EnqueueHistoricalMessagesAsync(cancellation);
			StartDispatchClientMessages(cancellation);
		}

		public void Stop()
		{
			messagesCancelltationSource.Cancel();
			List<Message> historicalMessages = new List<Message>();
			historicalMessages.AddRange(history.Skip(Math.Max(0, history.Count - Configuration.HistoryLimit)));

			repository.SaveMessages(historicalMessages);
			if (server.IsConnected)
			{
				var stopMessage = new Message
				{
					From = Configuration.ServerName,
					Body = Configuration.StopMessage,
					Code = MessageCode.Message
				};
				var stopTasks = new List<Task>();
				foreach (var client in clientContollers)
				{
					stopMessage.To = client.Key;
					client.Value.EnqueueMessage(stopMessage);
					stopTasks.Add(client.Value.Stop());
				}

				Task.WaitAll(stopTasks.ToArray());
			}

			serverHealthCancellationSource.Cancel();
		}

		private async Task EnqueueHistoricalMessagesAsync(CancellationToken cancellation)
		{
			await Task.Run(
				() =>
				{
					foreach (var message in repository.GetHistoricalMessages())
					{
						var recipient = message.To;
						if (!clientContollers.ContainsKey(recipient))
						{
							var controller = new ClientController(recipient, this);
							clientContollers.Add(recipient, controller);
						}

						clientContollers[recipient].EnqueueMessage(message);
					}
				},
				cancellation);
		}

		private void StartDispatchClientMessages(CancellationToken cancellation)
		{
			messagesDispatcher = new Thread(
				state =>
				{
					var token = (CancellationToken) state;
					while (!token.IsCancellationRequested)
					{
						var line = reader.ReadLine();
						if (line == null)
						{
							break;
						}

						translator.WriteLine($"message received ({line})");
						var message = Message.Deserialize(line);
						var from = message.From;
						if (!clientContollers.ContainsKey(from))
						{
							var controller = new ClientController(from, this);
							clientContollers.Add(from, controller);
						}

						switch (message.Code)
						{
							case MessageCode.Message:
								foreach (var to in clientContollers.Where(x => x.Key != from).ToArray())
								{
									var toMessage = new Message
									{
										From = from,
										To = to.Key,
										Body = message.Body,
										Code = MessageCode.Message
									};
									to.Value.EnqueueMessage(toMessage);
									history.Add(toMessage);
								}
								break;
							case MessageCode.Connect:
								clientContollers[from].Connect();
								break;
							case MessageCode.Disconnect:
								clientContollers[from].Disconnect();
								break;
							case MessageCode.None:
							default:
								break;
						}

					}
				})
			{
				IsBackground = true
			};
			messagesDispatcher.Start(cancellation);
		}

		private void StartServerHealthMonitor(CancellationToken cancellation)
		{
			healthMonitor = new Thread(
				state =>
				{
					var token = (CancellationToken) state;
					while (true)
					{
						token.ThrowIfCancellationRequested();
						WriteServerStatus();
						Thread.Sleep(1000);
					}
				})
			{
				IsBackground = true
			};
			healthMonitor.Start(cancellation);
		}

		public void Dispose()
		{
			messagesCancelltationSource?.Cancel();
			serverHealthCancellationSource?.Cancel();
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
				server?.Dispose();
			}
			catch { }
			finally
			{
				server = null;
			}

			messagesCancelltationSource?.Dispose();
			serverHealthCancellationSource?.Dispose();
		}

		private static readonly object SendLock = new object();
		public void SendMessage(string message)
		{
			Thread.Sleep(Configuration.ServerSendDelay);
			lock (SendLock)
			{
				writer.WriteLine(message);
				writer.Flush();
			}
		}

		private void WriteServerStatus()
		{
			translator.WriteLine("Server status:");
			foreach (var client in clientContollers.ToArray())
			{
				translator.WriteLine(
					$"{client.Key} ({client.Value.Status}): "
					+ $"messages in Q: {client.Value.MessagesInQueue} "
					+ $"thread state: {client.Value.ThreadState}");
			}
		}

		private void WriteServerConfiguration()
		{
			translator.WriteLine("Server configuration:");
			translator.WriteLine($"History file: {Configuration.MessagesHistoryFile}");
			translator.WriteLine($"History limit: {Configuration.HistoryLimit}");
			translator.WriteLine($"Server send delay: {Configuration.ServerSendDelay} ms");
		}
	}
}
