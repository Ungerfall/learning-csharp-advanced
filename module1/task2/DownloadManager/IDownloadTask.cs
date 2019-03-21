using System;
using System.Threading.Tasks;

namespace DownloadManager
{
	public interface IDownloadTask : IDisposable
	{
		void Cancel();
		Task<string> DownloadAsync(string address);
	}
}
