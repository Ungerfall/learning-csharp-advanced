namespace SummatorAsync
{
	public class Result
	{
		public Result(long sum, bool cancelled)
		{
			Sum = sum;
			Cancelled = cancelled;
		}

		public long Sum { get; }

		public bool Cancelled { get; }
	}
}
