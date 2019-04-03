using System.Collections.Generic;

namespace Messenger.Common.Data
{
	public interface IMessagesRepository
	{
		IEnumerable<Message> GetHistoricalMessages();
		void SaveMessages(IEnumerable<Message> messages);
	}
}
