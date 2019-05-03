using System;
using System.Threading;
using Utility.Logging;

namespace Documents.Core
{
	public abstract class Worker : IWorker
	{
		private Thread worker;

		protected readonly CancellationTokenSource CancellationSource = new CancellationTokenSource();

		public void Start()
		{
			worker = new Thread(
				state =>
				{
					try
					{
						var token = (CancellationToken) state;
						OnWorkerStart(token);
					}
					catch (Exception e)
					{
						SimpleLog.WriteLine(e.Message);
					}
				})
			{
				IsBackground = true
			};
			worker.Start(CancellationSource.Token);
		}

		public void Stop()
		{
			CancellationSource.Cancel();
			worker.Join();
			OnWorkerStop();
		}

		protected abstract void OnWorkerStart(CancellationToken token);
		protected abstract void OnWorkerStop();
	}
}