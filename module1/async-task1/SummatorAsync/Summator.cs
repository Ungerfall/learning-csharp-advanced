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

		internal Result Sum(uint n, CancellationToken cancellation)
		{
			if (n == 0u)
			{
				return new Result(0ul, false);
			}

			ulong sum = 1ul;
			uint i = 1u;
			do
			{
				if (cancellation.IsCancellationRequested)
				{
					return new Result(sum, true);
				}

				i++;
				checked
				{
					sum += i;
				}
			} while (i < n);

			return new Result(sum, false);
		}
	}
}
