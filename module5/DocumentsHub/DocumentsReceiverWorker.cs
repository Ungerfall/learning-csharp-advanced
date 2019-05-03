using System;
using System.Threading;
using Documents.Core;
using Messaging;
using Utility.Logging;

namespace DocumentsHub
{
	public class DocumentsReceiverWorker : Worker
	{
		private readonly Func<CancellationToken, IMessageConsumer<DocumentMessage>> documentsQueueFactory;
		private readonly IDocumentSaver documentSaver;

		public DocumentsReceiverWorker(Func<CancellationToken, IMessageConsumer<DocumentMessage>> messageQueueFactory, IDocumentSaver documentSaver)
		{
			documentsQueueFactory = messageQueueFactory
				?? throw new ArgumentNullException(nameof(messageQueueFactory));
			this.documentSaver = documentSaver ?? throw new ArgumentNullException(nameof(documentSaver));
		}

		protected override void OnWorkerStart(CancellationToken token)
		{
			var queue = documentsQueueFactory.Invoke(token);
			while (!token.IsCancellationRequested)
			{
				var message = queue.ReceiveMessage();
				documentSaver.Save(message.Name, message.Content);
				SimpleLog.WriteLine($"document {message.Name} saved");
			}
		}

		protected override void OnWorkerStop()
		{
		}
	}
}
