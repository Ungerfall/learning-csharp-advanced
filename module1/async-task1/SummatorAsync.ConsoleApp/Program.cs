using System;
using System.Threading;

namespace SummatorAsync.ConsoleApp
{
	class Program
	{
		const string exitString = "exit";

		static void Main(string[] args)
		{
			var summator = new Summator();
			Console.WriteLine("Summator v1.0");
			Console.WriteLine();

			var value = ProcessInput();
			while (true)
			{
				using (var cancellationSource = new CancellationTokenSource())
				{
					var cancellation = cancellationSource.Token;
					SumAndShowResultAsync(value, summator, cancellation);
					value = ProcessInput();
					cancellationSource.Cancel();
				}
			}
		}

		static async void SumAndShowResultAsync(uint value, Summator summator, CancellationToken cancellation)
		{
			int resultCursorTop = default(int);
			int resultCursorLeft = default(int);
			EnqueueConsoleAction(() =>
			{
				Console.Write("Calculating sum of {0} integers...Result: ", value);
				// save position to update result after calculation
				resultCursorTop = Console.CursorTop;
				resultCursorLeft = Console.CursorLeft;
				Console.WriteLine();
			});
			var result = await summator.SumAsync(value, cancellation);
			string report = result.Cancelled
				? "cancelled"
				: result.Sum.ToString();
			EnqueueConsoleAction(() =>
			{
				var cursorTop = Console.CursorTop;
				var cursorLeft = Console.CursorLeft;
				// update result, then go back to current position
				Console.SetCursorPosition(resultCursorLeft, resultCursorTop);
				Console.Write(report);
				Console.SetCursorPosition(cursorLeft, cursorTop);
			});
		}

		static uint ProcessInput()
		{
			string input = null;
			bool inputSucceeded = false;
			uint result;
			do
			{
				EnqueueConsoleAction(() =>
				{
					Console.WriteLine(
						"Enter unsigned integer [{0}..{1}] to calculate sum or type '{2}' to exit the program.",
						uint.MinValue,
						uint.MaxValue,
						exitString);
				});
				input = Console.ReadLine();
				if (input == exitString)
				{
					Environment.Exit(0);
				}

				inputSucceeded = uint.TryParse(input, out result);
			} while (!inputSucceeded);

			return result;
		}

		static readonly object consoleLock = new object();
		static void EnqueueConsoleAction(Action action)
		{
			lock (consoleLock)
			{
				action();
			}
		}
	}
}
