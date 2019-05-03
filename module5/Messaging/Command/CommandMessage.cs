using System;
using System.Text;
using Newtonsoft.Json;

namespace Messaging.Command
{
	[Serializable]
	public class CommandMessage<T>
	{
		public string CommandName { get; set; }

		public T Payload { get; set; }

		public virtual byte[] Serialize()
		{
			return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
		}

		public static CommandMessage<T> Deserialize(byte[] obj)
		{
			var objString = Encoding.UTF8.GetString(obj);
			return JsonConvert.DeserializeObject<CommandMessage<T>>(objString);
		}
	}

	public static class CommandMessageName
	{
		public const string SEND_STATUS_COMMAND_NAME = "UpdateStatus";
		public const string UPDATE_CONFIGURATION_COMMAND_NAME = "UpdateConfiguration";
	}
}