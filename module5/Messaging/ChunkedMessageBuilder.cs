using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Messaging
{
	public class ChunkedMessageBuilder
	{
		public IReadOnlyCollection<Chunk> Shred(byte[] message, int chunkSize)
		{
			var chunks = new List<Chunk>();
			var guid = Guid.NewGuid();
			var len = message.Length;
			if (len == 0)
				return chunks;

			int pos = 0;
			int i = 0;
			while (true)
			{
				var size = Math.Min(chunkSize, len - pos);
				var body = new ArraySegment<byte>(message, pos, size);
				pos += size;
				Chunk chunk = null;
				if (pos == len)
				{
					chunk = new Chunk(guid, i, chunkSize, body.ToArray(), true);
					chunks.Add(chunk);
					break;
				}

				chunk = new Chunk(guid, i, chunkSize, body.ToArray(), false);
				chunks.Add(chunk);
				i++;
			}

			return chunks;
		}

		public byte[] Build(List<Chunk> chunks)
		{
			if (chunks == null || !chunks.Any())
			{
				return new byte[0];
			}

			var orderedChunks = chunks.OrderBy(x => x.Position);
			var bodies = orderedChunks.Select(x => x.Body).ToArray();
			return Combine(bodies);
		}

		public static byte[] Combine(params byte[][] arrays)
		{
			byte[] ret = new byte[arrays.Sum(x => x.Length)];
			int offset = 0;
			foreach (byte[] data in arrays)
			{
				Buffer.BlockCopy(data, 0, ret, offset, data.Length);
				offset += data.Length;
			}

			return ret;
		}
	}
}