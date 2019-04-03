using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Common
{
	public class Configuration
	{
		public const string MESSENGER_PIPE = "messenger";
		public static string[] ClientNames = new string[5]
		{
			"Leonid",
			"Kirill",
			"Artem",
			"Alex",
			"Vadim"
		};

		public static string[] Messages = new string[10]
		{
			"Good morning",
			"Hello",
			"Good bye",
			"How are you",
			"Where are you going today?",
			"Stop!",
			"Come on",
			"Hmmm",
			":-)",
			"Okay"
		};

		public static string MessagesHistoryFile => "history.json";

		public static string StopMessage => "Server is shutting down...";

		public static string ServerName => "Server";

		public static int HistoryLimit => 10;

		public static int ServerSendDelay => 1;
	}
}
