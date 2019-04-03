using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Messenger.Common.Data
{
	public class MessagesFileRepository : IMessagesRepository
	{
		private string messagesHistoryFileFullPath
			= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Configuration.MessagesHistoryFile);

		public IEnumerable<Message> GetHistoricalMessages()
		{
			if (!File.Exists(messagesHistoryFileFullPath))
			{
				return Enumerable.Empty<Message>();
			}

			var fileContent = File.ReadAllText(messagesHistoryFileFullPath);
			return JsonConvert.DeserializeObject<List<Message>>(fileContent);
		}

		public void SaveMessages(IEnumerable<Message> messages)
		{
			if (!messages.Any())
				return;

			if (File.Exists(messagesHistoryFileFullPath))
			{
				var copy = $"{messagesHistoryFileFullPath}.{DateTime.Now.Ticks}.old";
				File.Move(messagesHistoryFileFullPath, copy);
			}

			var serialized = JsonConvert.SerializeObject(messages.ToList());
			File.WriteAllText(messagesHistoryFileFullPath, serialized);
		}
	}
}
