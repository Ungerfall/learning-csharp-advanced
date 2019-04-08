using System.Collections.Generic;
using System.IO;

namespace DocumentsJoiner
{
	public interface IDocumentsJoiner
	{
		Stream Join(IEnumerable<Document> documents);
	}
}
