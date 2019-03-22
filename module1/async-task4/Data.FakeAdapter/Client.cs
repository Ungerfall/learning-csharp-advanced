namespace Data.FakeAdapter
{
	public class Client : System.IEquatable<Client>
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public int Age { get; set; }

		public bool Equals(Client other)
		{
			return other != null && Id == other.Id;
		}

		public override bool Equals(object obj)
		{
			if (obj is Client == false)
				return false;

			return Equals(obj);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public override string ToString()
		{
			return new {Id = Id, Name = Name, Age = Age}.ToString();
		}
	}
}
