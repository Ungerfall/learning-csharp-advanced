namespace Messaging
{
	public interface IMessageProducer
	{
		void SendMessage(byte[] message);
	}
}