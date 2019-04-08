namespace DocumentsJoiner.Handlers
{
	public interface IDocumentHandler
	{
		bool Handle(Document document);
	}
}