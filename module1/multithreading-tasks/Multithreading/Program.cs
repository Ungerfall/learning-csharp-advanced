using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Multithreading
{
	class Program
	{
		static Dictionary<string, Action> problems = new Dictionary<string, Action>
		{
			["1"] = Task1,
			["2"] = Task2,
			["3"] = Task3,
			["4"] = Task4,
			["5"] = Task5,
			["6"] = Task6,
			["7"] = Task7
		};

		static void Main(string[] args)
		{
			var problemsString = string.Join(",", problems.Keys);
			while (true)
			{
				Console.WriteLine($"Problems: {problemsString}. Enter the number or type exit to exit");
				var input = Console.ReadLine();
				if (input == "exit")
				{
					break;
				}

				if (problems.Keys.Contains(input))
				{
					problems[input].Invoke();
				}
			}
		}

		/// <summary>
		/// Write a program, which creates an array of 100 Tasks, runs them and 
		/// wait all of them are not finished. Each Task should iterate from 1 
		/// to 1000 and print into the console the following string: 
		/// "Task #0 – {iteration number}".
		/// </summary>
		static void Task1()
		{
			const int tasksCount = 100;
			const int iterationsCount = 1000;

			var tasks = new Task[100];
			for (int i = 0; i < tasksCount; i++)
			{
				int taskNumber = i;
				var task = Task.Factory.StartNew(() =>
				{
					for (int j = 0; j < iterationsCount; j++)
					{
						Console.WriteLine($"Task #{taskNumber} - {j}");
					}
				});
				tasks[i] = task;
			}

			Task.WaitAll(tasks);
		}

		/// <summary>
		/// Write a program, which creates a chain of four Tasks. First Task – 
		/// creates an array of 10 random integer. Second Task – multiplies 
		/// this array with another random integer. Third Task – sorts this 
		/// array by ascending. Fourth Task – calculates the average value. 
		/// All this tasks should print the values to console
		/// </summary>
		static void Task2()
		{
			const int integersCount = 10;
			var integers = new int[integersCount];
			Action printIntegers = () => Console.WriteLine(string.Join(",", integers));

			Task.Factory.StartNew(
				() =>
				{
					var rng = new Random();
					for (int i = 0; i < integersCount; i++)
					{
						integers[i] = rng.Next();
					}

					printIntegers();
				})
			.ContinueWith(
				task =>
				{
					var rng = new Random();
					for (int i = 0; i < integersCount; i++)
					{
						integers[i] *= rng.Next();
					}

					printIntegers();
				})
			.ContinueWith(
				task =>
				{
					Array.Sort(integers);
					printIntegers();
				})
			.ContinueWith(
				task =>
				{
					Console.WriteLine(integers.Average());
				})
			.Wait();
		}

		/// <summary>
		/// Write a program, which multiplies two matrices and uses class Parallel.
		/// </summary>
		static void Task3()
		{
			Func<int, int[,]> initSquareMatrix = (x) =>
			{
				var matrix = new int[x, x];
				var rng = new Random();
				for (int i = 0; i < x; i++)
				{
					for (int j = 0; j < x; j++)
					{
						matrix[i, j] = rng.Next(256);
					}
				}

				return matrix;
			};

			const int size = 10;
			var matrix1 = initSquareMatrix(size);
			var matrix2 = initSquareMatrix(size);
			var multiplication = new long[size, size];

			Parallel.For(
				0,
				size,
				i =>
				{
					for (int j = 0; j < size; j++)
					{
						long mul = 0L;
						for (int k = 0; k < size; k++)
						{
							mul += matrix1[i, k] * matrix2[k, j];
						}

						multiplication[i, j] = mul;
					}
				});
		}

		/// <summary>
		/// Write a program which recursively creates 10 threads. Each thread 
		/// should be with the same body and receive a state with integer 
		/// number, decrement it, print and pass as a state into the newly 
		/// created thread. 
		/// Use Thread class for this task and Join for waiting threads.
		/// </summary>
		static void Task4()
		{

		}

		static void Task5()
		{

		}

		static void Task6()
		{

		}

		static void Task7()
		{

		}
	}
}
