using Messenger.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Server.ConsoleApp
{
	class Program
	{
		static Dictionary<string, List<string>> clientMessages = new Dictionary<string, List<string>>();

		static void Main(string[] args)
		{
			using (var server = new NamedPipeServerStream(Configuration.MESSENGER_PIPE, PipeDirection.InOut))
			using (StreamReader reader = new StreamReader(server))
			using (StreamWriter writer = new StreamWriter(server))
			{
				server.WaitForConnection();
				while (true)
				{
					var line = reader.ReadLine();
					if (line == null)
					{
						break;
					}

					Console.WriteLine(line);
					var message = Message.Deserialize(line);
					string clientName = message.ClientName;
					if (!clientMessages.ContainsKey(clientName))
					{
						clientMessages[clientName] = new List<string> { message.Body };
					}
					else
					{
						clientMessages[clientName].Add(message.Body);
					}

					foreach (var client in clientMessages.Where(kv => kv.Key != clientName))
					{
						var response = new Message { ClientName = client.Key, Body = message.Body };
						writer.WriteLine(response.Serialize());
						writer.Flush();
					}
				}
			}
		}
	}
}
