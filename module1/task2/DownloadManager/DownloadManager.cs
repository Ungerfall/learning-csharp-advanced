using System.Collections.Generic;

namespace DownloadManager
{
	public class DownloadManager
	{
		private List<IDownloadTask> downloadTasks = new List<IDownloadTask>();

		public IDownloadTask CreateNewDownloadTask()
		{
			var task = new DownloadTask();
			downloadTasks.Add(task);
			return task;
		}
	}
}
