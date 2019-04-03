using Messenger.Common.Data;
using System;

namespace Messenger.Server.ConsoleApp
{
	class Program
	{

		static void Main(string[] args)
		{
			var messagesRepo = new MessagesFileRepository();
			using (var server = new MessengerServer(messagesRepo, Console.Out))
			{
				try
				{
					server.StartAsync();
					while (true)
					{
						Console.WriteLine("Type 'stop' to exit the program");
						var input = Console.ReadLine();
						if (input == "stop")
						{
							server.Stop();
							break;
						}
					}
				}
				catch (Exception)
				{
					server.Stop();
				}
			}
		}
	}
}
