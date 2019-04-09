﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DocumentsJoiner.Handlers;
using DocumentsJoiner.IO;
using DocumentsJoiner.Utility;

namespace DocumentsJoiner
{
	public class DocumentsController
	{
		private readonly SortedDictionary<int, IDocumentHandler> handlersChain;
		private readonly IDocumentErrorHandler exceptionsHandler;
		private readonly IWaitForFile fileReader;
		private readonly int timeout;

		private Timer timeoutTimer;
		private ManualResetEvent collectOnTimeoutEvent = new ManualResetEvent(true);

		public DocumentsController(
			Func<DocumentsController, SortedDictionary<int, IDocumentHandler>> handlersChainFactory,
			IDocumentErrorHandler exceptionsHandler,
			IWaitForFile fileReader,
			int timeout)
		{
			if (handlersChainFactory == null) throw new ArgumentNullException(nameof(handlersChainFactory));
			if (timeout <= 0) throw new ArgumentOutOfRangeException(nameof(timeout));
			this.exceptionsHandler = exceptionsHandler ?? throw new ArgumentNullException(nameof(exceptionsHandler));
			this.fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
			this.timeout = timeout;

			handlersChain = handlersChainFactory.Invoke(this);
			timeoutTimer = new Timer(CollectBatch, null, Timeout.Infinite, Timeout.Infinite);
		}

		public event EventHandler<DocumentBatchEventArgs> DocumentsBatchCollected;

		public void HandleCandidateDocument(string filepath)
		{
			collectOnTimeoutEvent.WaitOne();
			Document document = null;
			FileStream stream = null;
			try
			{
				stream = fileReader.AttemptToReadFile(filepath);
				document = new Document(filepath, stream);
				foreach (var handler in handlersChain)
				{
					if (handler.Value.Handle(document))
					{
						break;
					}
				}
			}
			catch (Exception e)
			{
				SimpleLog.WriteLine($"exception during handling {e.Message}");
				if (document != null)
				{
					exceptionsHandler.Handle(document, e);
				}
			}
			finally
			{
				stream?.Dispose();
				timeoutTimer.Change(timeout, Timeout.Infinite);
			}
		}

		public void AddDocumentToBatch(Document document)
		{
			if (CurrentBatch == null)
			{
				CurrentBatch = new DocumentBatch(document.Number, document.Prefix);
			}

			CurrentBatch.Add(document);
		}

		public void CollectBatch()
		{
			OnDocumentsBatchCollected();
			CurrentBatch = null;
		}

		private void CollectBatch(object state)
		{
			collectOnTimeoutEvent.Reset();
			SimpleLog.WriteLine("timeout");
			CollectBatch();
			collectOnTimeoutEvent.Set();
		}

		protected virtual void OnDocumentsBatchCollected()
		{
			if (CurrentBatch != null && CurrentBatch.Documents.Any())
			{
				DocumentsBatchCollected?.Invoke(this, new DocumentBatchEventArgs {DocumentBatch = CurrentBatch});
			}
		}

		public void InitializeNewBatch(Document document)
		{
			OnDocumentsBatchCollected();
			CurrentBatch = new DocumentBatch(document.Number, document.Prefix);
		}

		public DocumentBatch CurrentBatch { get; private set; }
	}
}