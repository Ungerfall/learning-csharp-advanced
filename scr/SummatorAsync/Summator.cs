using System.Threading;
using System.Threading.Tasks;

namespace SummatorAsync
{
	public class Summator
	{
		public async Task<Result> SumAsync(uint n, CancellationToken cancellation)
		{
			return await Task.Factory.StartNew(
				() =>
				{
					return Sum(n, cancellation);
				},
				cancellation)
				.ConfigureAwait(false);
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
			} while (i <= n);

			return new Result(sum, false);
		}
	}
}
