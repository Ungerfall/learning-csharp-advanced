namespace SummatorAsync
{
	public class Result
	{
		public Result(ulong sum, bool cancelled)
		{
			Sum = sum;
			Cancelled = cancelled;
		}

		public ulong Sum { get; }

		public bool Cancelled { get; }
	}
}
