using System.Runtime.CompilerServices;

namespace Utility.Logging
{
	public static class SimpleLog
	{
		private static readonly Logger logger = new Logger();

		public static void WriteLine(string text, [CallerMemberName] string caller = null)
		{
			logger.Log($"{caller ?? string.Empty}: {text}");
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
}