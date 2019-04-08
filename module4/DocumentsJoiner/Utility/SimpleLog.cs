namespace DocumentsJoiner.Utility
{
	public static class SimpleLog
	{
		private static readonly Logger logger = new Logger();

		public static void WriteLine(string text)
		{
			logger.Log(text);
		}

		private class Logger : ILog
		{
			public Logger()
			{
			}

			public void Log(string text)
			{
				System.Diagnostics.Trace.WriteLine(text);
			}
		}
	}

	public interface ILog
	{
		void Log(string text);
	}
}