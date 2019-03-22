using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace DownloadManager
{
	public class DownloadTask : IDownloadTask
	{
		private CancellationTokenSource cancellationSource;

		public async Task<string> DownloadAsync(string address)
		{
			var cancellation = CreateNewToken();
			using (var webClient = new WebClient())
			using (cancellation.Register(webClient.CancelAsync))
			{
				//await Task.Delay(10000, cancellation);
				return await webClient.DownloadStringTaskAsync(address).ConfigureAwait(false);
			}
		}

		public void Cancel()
		{
			cancellationSource.Cancel();
		}

		private CancellationToken CreateNewToken()
		{
			if (cancellationSource != null)
			{
				cancellationSource?.Dispose();
				cancellationSource = null;
			}

			cancellationSource = new CancellationTokenSource();
			return cancellationSource.Token;
		}

		public void Dispose()
		{
			cancellationSource?.Dispose();
		}
	}
}
