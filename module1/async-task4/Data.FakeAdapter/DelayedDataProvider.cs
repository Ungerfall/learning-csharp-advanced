using System;
using System.Threading;

namespace Data.FakeAdapter
{
	/// <summary>
	/// Implements fake <see cref="IDataProvider"/>, which simulates synchronous long work.
	/// </summary>
	public class DelayedDataProvider : IDataProvider
	{
		private const int DELAY_MS = 10000;

		private readonly Action<string> logger;

		public DelayedDataProvider(Action<string> logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			this.logger = logger;
		}

		public object Execute(string command, object data)
		{
			Thread.Sleep(DELAY_MS);
			logger($"{command} executed with data {data}");
			return data;
		}
	}
}
