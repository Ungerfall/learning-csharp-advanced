using System;

namespace Messenger.Client
{
	public interface IMessengerClient
	{
		string Name { get; }
		void Connect();
		void Disconnect();
		void SendMessage(string message);
		Action<string> ReceiveMessageFromServer { get; }
	}
}
