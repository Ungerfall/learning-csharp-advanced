namespace Data.FakeAdapter
{
	public class ClientRepositoryAsync : RepositoryAsyncBase<Client>
	{
		public ClientRepositoryAsync(System.Action<string> logger) : base(new DelayedDataProvider(logger))
		{
		}

		protected override string CreateCommand => "create command";

		protected override string ReadCommand => "read command";

		protected override string UpdateCommand => "update command";

		protected override string DeleteCommand => "delete command";
	}
}
