using Messenger.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Messenger.Client.ConsoleApp
{
	class Program
	{
		static readonly Random Random = new Random();

		static void Main(string[] args)
		{
			using (var broker = new MessegesBroker())
			{
				broker.Start();
				while (true)
				{
					var name = Configuration.ClientNames[Random.Next(5)];
					var client = new MessengerClient(name, broker, Console.Out);
					client.Connect();
					for (int i = 0; i < Random.Next(1,10); i++)
					{
						Thread.Sleep(Random.Next(1000, 5000));
						var message = Configuration.Messages[Random.Next(10)];
						client.SendMessage(message);
					}

					client.Disconnect();
				}
			}
		}
	}
}
