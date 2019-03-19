using System.Threading;
using System.Threading.Tasks;

namespace SummatorAsync
{
	public class Summator
	{
		public async Task<Result> SumAsync(uint n, CancellationToken cancellation)
		{
			var sumTask = new Task<Result>(() =>
			{
				return Sum(n, cancellation);
			});
			sumTask.Start();
			var sum = await sumTask.ConfigureAwait(false);

			return sum;
		}

		private Result Sum(uint n, CancellationToken cancellation)
		{
			long sum = 0;
			uint i = 1;
			do
			{
				if (cancellation.IsCancellationRequested)
				{
					return new Result(sum, true);
				}

				checked
				{
					sum += i;
				}

				i++;
			} while (i < n);

			return new Result(sum, false);
		}
	}
}
