using Messenger.Common;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Messenger.Client.ConsoleApp
{
	class Program
	{
		static readonly Random Random = new Random();

		static void Main(string[] args)
		{
			using (var broker = new MessegesBroker(Console.Out))
			{
				broker.Start();
				List<MessengerClient> clients = new List<MessengerClient>();
				int clientsCount = Configuration.ClientNames.Length;
				int messagesCount = Configuration.Messages.Length;
				foreach (var name in Configuration.ClientNames)
				{
					var client = new MessengerClient(name, broker, Console.Out);
					client.Connect();
					clients.Add(client);
				}

				while (broker.IsConnected)
				{
					var client = clients[Random.Next(clientsCount)];
					client.Connect();
					for (int i = 0; i < Random.Next(1, messagesCount); i++)
					{
						Thread.Sleep(Random.Next(1000, 3000));
						var message = Configuration.Messages[Random.Next(messagesCount)];
						client.SendMessage(message);
					}

					client.Disconnect();
				}
			}
		}
	}
}
