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
			var cancellationSource = new CancellationTokenSource();
			var cancellation = cancellationSource.Token;
			SumAndShowResultAsync(value, summator, cancellation);
			while (true)
			{
				value = ProcessInput();
				cancellationSource.Cancel();
				cancellationSource = new CancellationTokenSource();
				cancellation = cancellationSource.Token;
				SumAndShowResultAsync(value, summator, cancellation);
			}
		}

		static async void SumAndShowResultAsync(uint value, Summator summator, CancellationToken cancellation)
		{
			Console.Write("Calculating sum of {0} integers...Result: ", value);
			// save position to update result after calculation
			var resultCursorTop = Console.CursorTop;
			var resultCursorLeft = Console.CursorLeft;
			Console.WriteLine();
			var sum = await summator.SumAsync(value, cancellation);
			string result = cancellation.IsCancellationRequested
				? "cancelled"
				: sum.ToString();
			var cursorTop = Console.CursorTop;
			var cursorLeft = Console.CursorLeft;
			// update result, then go back to current position
			Console.SetCursorPosition(resultCursorLeft, resultCursorTop);
			Console.Write(result);
			Console.SetCursorPosition(cursorLeft, cursorTop);
		}

		static uint ProcessInput()
		{
			string input;
			bool inputSucceeded = false;
			uint result;
			do
			{
				Console.WriteLine(
					"Enter unsigned integer [{0}..{1}] to calculate sum or type '{2}' to exit the program.",
					uint.MinValue,
					uint.MaxValue,
					exitString);
				input = Console.ReadLine();
				if (input == exitString)
				{
					Environment.Exit(0);
				}

				inputSucceeded = uint.TryParse(input, out result);
			} while (!inputSucceeded);

			return result;
		}
	}
}