using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Messenger.Common
{
	public static class QueueExtensions
	{
		public static IEnumerable<T> DequeueAll<T>(this ConcurrentQueue<T> queue)
		{
			T item;
			while (queue.TryDequeue(out item))
			{
				yield return item;
			}
		}
	}
}
