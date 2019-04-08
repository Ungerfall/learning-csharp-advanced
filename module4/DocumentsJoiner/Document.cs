using System.IO;

namespace DocumentsJoiner
{
	public class Document
	{
		public Document(string fullPath, Stream stream)
		{
			FullPath = fullPath;
			Stream = stream;
			Name = Path.GetFileName(FullPath);
		}

		public string FullPath { get; }

		public string Name { get; }

		public string Prefix { get; set; }

		public int Number { get; set; }

		public string Barcode { get; set; }

		public Stream Stream { get; }
	}
}