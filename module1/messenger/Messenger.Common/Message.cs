using Newtonsoft.Json;

namespace Messenger.Common
{
	public class Message
	{
		public MessageCode Code { get; set; }

		public string From { get; set; }

		public string To { get; set; }

		public string Body { get; set; }

		public string Serialize()
		{
			return JsonConvert.SerializeObject(this);
		}

		public static Message Deserialize(string obj)
		{
			return JsonConvert.DeserializeObject<Message>(obj);
		}
	}

	public enum MessageCode
	{
		None = 0,
		Message = 1,
		Connect = 2,
		Disconnect = 3
	}
}
