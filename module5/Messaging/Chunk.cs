using System;

namespace Messaging
{
	public class Chunk : IEquatable<Chunk>
	{
		public static Chunk NullChunk = new Chunk(Guid.Empty, -1, -1, new byte[0], false);

		public Chunk(Guid guid, int position, int size, byte[] body, bool isLast)
		{
			Guid = guid;
			Position = position;
			Size = size;
			Body = body;
			IsLast = isLast;
		}

		public Guid Guid { get; }

		public int Position { get; }

		public int Size { get; }

		public byte[] Body { get; }

		public bool IsLast { get; }

		public bool Equals(Chunk other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Guid.Equals(other.Guid);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Chunk) obj);
		}

		public override int GetHashCode()
		{
			return Guid.GetHashCode();
		}
	}
}