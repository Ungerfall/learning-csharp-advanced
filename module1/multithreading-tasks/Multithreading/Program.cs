using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

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

			for (int i = 0; i < size; i++)
			{
				for (int j = 0; j < size - 1; j++)
				{
					Console.Write($"{multiplication[i, j]} ");
				}

				Console.Write(multiplication[i, size - 1]);
				Console.WriteLine();
			}
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
			const int initialState = 10;
			Task4_CreateThreadRecursively(initialState);
		}

		static void Task4_CreateThreadRecursively(int state)
		{
			if (state <= 0)
				return;

			var thread = new Thread(threadState =>
			{
				var threadStateInt = (int)threadState;
				--threadStateInt;
				Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId}; state = {threadStateInt}");
				Task4_CreateThreadRecursively(threadStateInt);
			});
			thread.Start(state);
			thread.Join();
		}

		/// <summary>
		/// Write a program which recursively creates 10 threads. Each thread 
		/// should be with the same body and receive a state with integer 
		/// number, decrement it, print and pass as a state into the newly 
		/// created thread. 
		/// Use ThreadPool class for this task and Semaphore for waiting threads.
		/// </summary>
		static void Task5()
		{
			const int semaphorePoolCount = 1;
			pool = new Semaphore(0, semaphorePoolCount);
			const int initialState = 10;
			Task5_CreateThreadRecursively(initialState);
			pool.Release(semaphorePoolCount);
		}

		static Semaphore pool;
		static void Task5_CreateThreadRecursively(int state)
		{
			if (state <= 0)
				return;

			ThreadPool.QueueUserWorkItem(
				threadState =>
				{
					pool.WaitOne();
					var threadStateInt = (int)threadState;
					--threadStateInt;
					Console.WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId}; state = {threadStateInt}");
					Task5_CreateThreadRecursively(threadStateInt);
					pool.Release();
				},
				state);
		}

		static List<int> sharedThreadUnsafeCollection;
		/// <summary>
		/// Write a program which creates two threads and a shared collection: 
		/// the first one should add 10 elements into the collection and the 
		/// second should print all elements in the collection after each 
		/// adding. 
		/// Use Thread, ThreadPool or Task classes for thread creation and any 
		/// kind of synchronization constructions.
		/// </summary>
		static void Task6()
		{
			using (var addWait = new ManualResetEventSlim(false))
			using (var printWait = new ManualResetEventSlim(true))
			using (var endPrintingWait = new ManualResetEventSlim(false))
			{
				sharedThreadUnsafeCollection = new List<int>(10);
				var rng = new Random();
				ThreadPool.QueueUserWorkItem(state =>
				{
					while (sharedThreadUnsafeCollection.Count < sharedThreadUnsafeCollection.Capacity)
					{
						printWait.Wait();
						//Thread.Sleep(1000);
						sharedThreadUnsafeCollection.Add(rng.Next(256));
						addWait.Set();
						printWait.Reset();
					}
					addWait.Set();
				});
				ThreadPool.QueueUserWorkItem(state =>
				{
					while (addWait.WaitHandle.WaitOne(3000))
					{
						Console.WriteLine(string.Join(",", sharedThreadUnsafeCollection));
						printWait.Set();
						addWait.Reset();
					}
					endPrintingWait.Set();
				});

				endPrintingWait.Wait();
			}
		}

		static readonly Random Random = new Random();
		/// <summary>
		/// Create a Task and attach continuations to it according to the 
		/// following criteria:
		/// a. Continuation task should be executed regardless of the result of 
		/// the parent task.
		/// b. Continuation task should be executed when the parent task 
		/// finished without success.
		/// c. Continuation task should be executed when the parent task would 
		/// be finished with fail and parent task thread should be reused for 
		/// continuation
		/// d. Continuation task should be executed outside of the thread pool 
		/// when the parent task would be cancelled
		/// </summary>
		static void Task7()
		{
			var continuationStrategies = new Dictionary<int, Action>
			{
				[0] = () =>
				{
					Task.Factory.StartNew(
						() =>
						{
							if (Random.Next() % 2 == 1)
							{
								Console.WriteLine($"Task will be completed with status {TaskStatus.RanToCompletion}");
								return;
							}
							else
							{
								Console.WriteLine($"Task will be completed with status {TaskStatus.Faulted}");
								throw new Exception();
							}
						})
					.ContinueWith(
						antecedent =>
						{
							Console.WriteLine($"Task continued regardless of state ({antecedent.Status})");
						}
						,TaskContinuationOptions.None);
				},
				[1] = () =>
				{
					var tokenSource = new CancellationTokenSource();
					var token = tokenSource.Token;
					Task.Factory.StartNew(
						() =>
						{
							var choice = Random.Next(3);
							if (choice == 0)
							{
								tokenSource.Cancel();
								Console.WriteLine($"Task will be completed with status {TaskStatus.Canceled}");
								token.ThrowIfCancellationRequested();
							}
							else if (choice == 1)
							{
								Console.WriteLine($"Task will be completed with status {TaskStatus.Faulted}");
								throw new Exception();
							}
							else
							{
								Console.WriteLine($"Task will be completed with status {TaskStatus.RanToCompletion}");
								return;
							}
						},
						token)
					.ContinueWith(
						antecedent =>
						{
							if (antecedent.Status == TaskStatus.RanToCompletion)
							{
								throw new Exception(
									"Continuation ran on RanToCompletion, "
									+ "however TaskContinuationOptions are set to NotOnRanToCompletion");
							}

							Console.WriteLine($"Task continued with status {antecedent.Status}");
						},
						TaskContinuationOptions.NotOnRanToCompletion);
				},
				[2] = () =>
				{
					var tokenSource = new CancellationTokenSource();
					var token = tokenSource.Token;
					var callerThreadId = Thread.CurrentThread.ManagedThreadId;
					int? taskThreadId = null;
					Task.Factory.StartNew(
						() =>
						{
							taskThreadId = Thread.CurrentThread.ManagedThreadId;
							var choice = Random.Next(3);
							if (choice == 0)
							{
								tokenSource.Cancel();
								Console.WriteLine($"Task will be completed with status {TaskStatus.Canceled}");
								token.ThrowIfCancellationRequested();
							}
							else if (choice == 1)
							{
								Console.WriteLine($"Task completed with status {TaskStatus.Faulted}");
								throw new Exception();
							}
							else
							{
								Console.WriteLine($"Task completed with status {TaskStatus.RanToCompletion}");
								return;
							}
						},
						token)
					.ContinueWith(
						antecedent =>
						{
							if (antecedent.Status != TaskStatus.Faulted)
							{
								throw new Exception(
									$"Continuation ran on {antecedent.Status}, "
									+ "however TaskContinuationOptions are set to OnlyOnFaulted");
							}

							var continuationThreadId = Thread.CurrentThread.ManagedThreadId;
							if (continuationThreadId != callerThreadId && continuationThreadId != taskThreadId)
							{
								throw new Exception(
									"Continuation ran on different thread "
									+"regardless of ExecuteSynchronously option");
							}

							Console.WriteLine($"Task continued with status {antecedent.Status}");
						},
						TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
				},
				[3] = () =>
				{
					var tokenSource = new CancellationTokenSource();
					var token = tokenSource.Token;
					int? taskThreadId = null;
					Task.Factory.StartNew(
						() =>
						{
							taskThreadId = Thread.CurrentThread.ManagedThreadId;
							var choice = Random.Next(3);
							if (choice == 0)
							{
								tokenSource.Cancel();
								Console.WriteLine($"Task will be completed with status {TaskStatus.Canceled}");
								token.ThrowIfCancellationRequested();
							}
							else if (choice == 1)
							{
								Console.WriteLine($"Task completed with status {TaskStatus.Faulted}");
								throw new Exception();
							}
							else
							{
								Console.WriteLine($"Task completed with status {TaskStatus.RanToCompletion}");
								return;
							}
						},
						token)
					.ContinueWith(
						antecedent =>
						{
							if (antecedent.Status != TaskStatus.Canceled)
							{
								throw new Exception(
									$"Continuation ran on {antecedent.Status}, "
									+ "however TaskContinuationOptions are set to OnlyOnCanceled");
							}

							var continuationThreadId = Thread.CurrentThread.ManagedThreadId;
							if (continuationThreadId == taskThreadId)
							{
								throw new Exception(
									"Continuation ran on same thread as antecedent thread"
									+"regardless of RunContinuationsAsynchronously option");
							}

							Console.WriteLine($"Task continued with status {antecedent.Status}");
						},
						TaskContinuationOptions.OnlyOnCanceled
							| TaskContinuationOptions.RunContinuationsAsynchronously);
				}
			};
			continuationStrategies[Random.Next(3)].Invoke();
		}
	}
}
