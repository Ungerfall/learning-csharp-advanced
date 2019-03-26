using Newtonsoft.Json;

namespace Messenger.Common
{
	public class Message
	{
		public string ClientName { get; set; }

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
}
