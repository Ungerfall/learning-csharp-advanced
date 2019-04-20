using System.Collections.Generic;
using System.Linq;

namespace DocumentsJoiner
{
	public class DocumentBatch
	{
		private readonly SortedDictionary<int, Document> documents = new SortedDictionary<int, Document>();

		public DocumentBatch(int startNumber, string prefix)
		{
			Prefix = prefix;
			MinSequenceNumber = startNumber;
		}

		public void Add(Document document)
		{
			documents.Add(document.Number, document);
		}

		public IEnumerable<Document> Documents => documents.Values;

		public string Prefix { get; }

		public int MinSequenceNumber { get; }

		public int MaxSequenceNumber => documents.Keys.Max();
	}
}