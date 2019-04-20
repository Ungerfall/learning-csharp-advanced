using System;
using System.Threading;
using Messaging;
using Utility.Logging;

namespace DocumentsHub
{
	public class DocumentsReceiverWorker
	{
		private readonly IMessageConsumer<DocumentMessage> documentsQueue;
		private readonly IDocumentSaver documentSaver;
		private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();

		private Thread worker;

		public DocumentsReceiverWorker(IMessageConsumer<DocumentMessage> messageQueue, IDocumentSaver documentSaver)
		{
			this.documentsQueue = messageQueue ?? throw new ArgumentNullException(nameof(messageQueue));
			this.documentSaver = documentSaver ?? throw new ArgumentNullException(nameof(documentSaver));
		}

		public void Start()
		{
			worker = new Thread(
				state =>
				{
					try
					{
						var token = (CancellationToken) state;
						while (!token.IsCancellationRequested)
						{
							var message = documentsQueue.ReceiveMessage();
							documentSaver.Save(message.Name, message.Content);
							SimpleLog.WriteLine($"document {message.Name} saved");
						}
					}
					catch (Exception e)
					{
						SimpleLog.WriteLine($"exception: {e.Message}");
					}
				})
			{
				IsBackground = true
			};
			worker.Start(cancellationSource.Token);
		}

		public void Stop()
		{
			cancellationSource.Cancel();
			worker.Join();
		}
	}
}
