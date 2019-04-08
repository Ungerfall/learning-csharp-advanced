using System;
using System.IO;
using System.Threading;

namespace DocumentsJoiner.IO
{
	public class WaitForFile : IWaitForFile
	{
		private readonly int attempts;

		public WaitForFile(int attempts)
		{
			if (attempts <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(attempts));
			}

			this.attempts = attempts;
		}

		public FileStream AttemptToReadFile(string fullPath)
		{
			int i = 0;
			do
			{
				i++;
				FileStream fs = null;
				try
				{
					fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.None);
					return fs;
				}
				catch (IOException)
				{
					fs?.Dispose();
					Thread.Sleep(50);
					if (i == attempts)
						throw;
				}
			} while (i < attempts);

			return null;
		}
	}
}