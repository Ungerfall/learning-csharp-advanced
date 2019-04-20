using System;
using System.IO;
using System.Threading;

namespace DocumentsJoiner.IO
{
	public class WaitForFile : IWaitForFile
	{
		private readonly int attempts;
		private readonly int period;

		public WaitForFile(int attempts, int period)
		{
			if (attempts <= 0) throw new ArgumentOutOfRangeException(nameof(attempts));
			if (period <= 0) throw new ArgumentOutOfRangeException(nameof(period));

			this.attempts = attempts;
			this.period = period;
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
					Thread.Sleep(period);
					if (i == attempts)
						throw;
				}
			} while (i < attempts);

			return null;
		}
	}
}