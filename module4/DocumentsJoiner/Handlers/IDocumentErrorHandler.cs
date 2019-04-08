using System;

namespace DocumentsJoiner.Handlers
{
	public interface IDocumentErrorHandler
	{
		void Handle(Document document, Exception exception);
	}
}