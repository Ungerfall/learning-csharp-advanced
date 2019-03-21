using NUnit.Framework;
using System.Threading;

namespace SummatorAsync
{
	[TestFixture]
	internal class SummatorTest
	{
		private Summator summator = new Summator();

		[Test]
		[TestCase(uint.MinValue, uint.MinValue)]
		[TestCase(uint.MaxValue, 9223372034707292160ul)]
		public void TestSum(uint n, ulong sum)
		{
			Assert.AreEqual(summator.Sum(n, default(CancellationToken)).Sum, sum);
			Assert.AreEqual(summator.Sum(n, default(CancellationToken)).Sum, n * ((ulong)n + 1) / 2);
		}
	}
}
