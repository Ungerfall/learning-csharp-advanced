namespace Messaging
{
	public interface IMessageConsumer<out T>
	{
		T ReceiveMessage();
	}
}
