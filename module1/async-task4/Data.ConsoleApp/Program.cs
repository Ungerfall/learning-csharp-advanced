using System;
using System.Collections.Generic;
using Data.FakeAdapter;

namespace Data.ConsoleApp
{
	class Program
	{
		static IRepositoryAsync<Client> repository = new ClientRepositoryAsync(LogToConsole);
		static Dictionary<string, Action> commandToAction = new Dictionary<string, Action>
		{
			["create"] = async () => await repository.CreateAsync(new Client()),
			["read"] = async () => await repository.ReadAsync(new Client()),
			["update"] = async () => await repository.UpdateAsync(new Client()),
			["delete"] = async () => await repository.DeleteAsync(new Client())
		};

		static void Main(string[] args)
		{
			var command = ProcessInput();
			while (true)
			{
				commandToAction[command].Invoke();
				command = ProcessInput();
			}
		}

		static void ShowMenu()
		{
			EnqueueConsoleWrite("Operation: create, read, update, delete. Type exit to exit");
		}

		static string ProcessInput()
		{
			string input = null;
			bool inputSucceeded = false;
			do
			{
				ShowMenu(); 
				input = Console.ReadLine();
				if (input == "exit")
				{
					Environment.Exit(0);
				}

				inputSucceeded = commandToAction.ContainsKey(input);

			} while (!inputSucceeded);

			return input;
		}

		static void LogToConsole(string log)
		{
			EnqueueConsoleWrite(log);
		}

		static readonly object consoleLock = new object();
		static void EnqueueConsoleWrite(string message)
		{
			lock (consoleLock)
			{
				Console.WriteLine(message);
			}
		}
	}
}
