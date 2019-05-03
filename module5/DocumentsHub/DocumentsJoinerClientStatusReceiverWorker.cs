using System;
using System.IO;
using System.Threading;
using Documents.Core;
using Messaging;
using Utility.Logging;

namespace DocumentsHub
{
	public class DocumentsJoinerClientStatusReceiverWorker : Worker
	{
		private const int NULL_MESSAGE_WAIT_MS = 3000;

		public static readonly string ClientStatusFileFullPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"status");

		private readonly IMessageConsumer<string> statusesQueue;

		public DocumentsJoinerClientStatusReceiverWorker(IMessageConsumer<string> statusesQueue)
		{
			this.statusesQueue = statusesQueue ?? throw new ArgumentNullException(nameof(statusesQueue));
			try
			{
				File.Delete(ClientStatusFileFullPath);
			}
			catch
			{
			}
		}

		protected override void OnWorkerStart(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				var messageLine = statusesQueue.ReceiveMessage();
				if (messageLine == null)
				{
					token.WaitHandle.WaitOne(NULL_MESSAGE_WAIT_MS);
					continue;
				}

				File.AppendAllText(ClientStatusFileFullPath, messageLine + Environment.NewLine);
				SimpleLog.WriteLine(messageLine);
			}
		}

		protected override void OnWorkerStop()
		{
		}
	}
}