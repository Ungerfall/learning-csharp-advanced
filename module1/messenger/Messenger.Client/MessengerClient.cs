using System;
using System.IO;

namespace Messenger.Client
{
	public class MessengerClient : IMessengerClient
	{
		private readonly MessegesBroker broker;
		private readonly TextWriter outputWriter;

		public MessengerClient(string name, MessegesBroker broker, TextWriter outputWriter)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
			if (broker == null) throw new ArgumentNullException(nameof(broker));
			if (outputWriter == null) throw new ArgumentNullException(nameof(outputWriter));

			Name = name;
			this.broker = broker;
			this.outputWriter = outputWriter;
		}

		public string Name { get; private set; }

		public Action<string> ReceiveMessageFromServer => WriteOutput;

		public void Connect()
		{
			broker.Subscribe(this);
		}

		public void Disconnect()
		{
			broker.Unsubscribe(this);
		}

		public void SendMessage(string message)
		{
			broker.EnqueueMessage(Name, message, Common.MessageCode.Message);
		}

		private void WriteOutput(string msg)
		{
			outputWriter.WriteLine(msg);
		}
	}
}
