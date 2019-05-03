using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Documents.Core;
using DocumentsJoiner.Configuration;
using Messaging;

namespace DocumentsJoiner
{
	public class DocumentsJoinerStatusObserverWorker : Worker
	{
		private const int SENDING_STATUS_PERIOD_MS = 1 * 3000;

		private readonly IEnumerable<IStatusObservable> observables;
		private readonly IMessageProducer statusQueue;

		public DocumentsJoinerStatusObserverWorker(
			IEnumerable<IStatusObservable> observables,
			IMessageProducer statusQueue)
		{
			this.observables = observables ?? throw new ArgumentNullException(nameof(observables));
			this.statusQueue = statusQueue ?? throw new ArgumentNullException(nameof(statusQueue));
		}

		protected override void OnWorkerStart(CancellationToken token)
		{
			while (true)
			{
				if (token.IsCancellationRequested)
				{
					PublishStatusMessages();
					break;
				}

				PublishStatusMessages();
				token.WaitHandle.WaitOne(SENDING_STATUS_PERIOD_MS);
			}
		}

		protected override void OnWorkerStop()
		{
		}

		private static readonly object Lock = new object();
		public void PublishStatusMessages()
		{
			lock (Lock)
			{
				foreach (var statusObservable in observables)
				{
					var statusWithPId = $"{Process.GetCurrentProcess().Id}: {statusObservable.Status}";
					var configurationStatusDto = new
					{
						ConfigurationContext.Timeout,
						ConfigurationContext.BarcodeSeparator
					};
					var msg = $"{statusWithPId}; cfg: {configurationStatusDto}";
					statusQueue.SendMessage(DocumentsJoinerStatus.Encoding.GetBytes(msg));
				}
			}
		}
	}
}