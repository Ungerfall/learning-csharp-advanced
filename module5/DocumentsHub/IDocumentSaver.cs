namespace DocumentsHub
{
	public interface IDocumentSaver
	{
		void Save(string name, byte[] content);
	}
}