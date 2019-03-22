namespace Data
{
	public interface IDataProvider
	{
		object Execute(string command, object data);
	}
}
