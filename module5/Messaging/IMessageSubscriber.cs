using System;

namespace Messaging
{
	public interface IMessageSubscriber
	{
		void Subscribe(Action<byte[]> action);
	}
}