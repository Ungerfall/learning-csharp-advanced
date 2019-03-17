using System.Threading;
using System.Threading.Tasks;

namespace SummatorAsync
{
	public class Summator
	{
		public async Task<long> SumAsync(uint n, CancellationToken cancellation)
		{
			var sumTask = new Task<long>(() =>
			{
				return Sum(n, cancellation);
			});
			sumTask.Start();
			var sum = await sumTask.ConfigureAwait(false);

			return sum;
		}

		private long Sum(uint n, CancellationToken cancellation)
		{
			long sum = 0;
			uint i = 1;
			do
			{
				if (cancellation.IsCancellationRequested)
				{
					return sum;
				}

				checked
				{
					sum += i;
				}

				i++;
			} while (i < n);

			return sum;
		}
	}
}
