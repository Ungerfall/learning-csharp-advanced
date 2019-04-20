using System.IO;

namespace DocumentsHub
{
	public class SimpleFileSaver : IDocumentSaver
	{
		private readonly string folderPath;

		public SimpleFileSaver(string folderPath)
		{
			this.folderPath = folderPath;
		}

		public void Save(string name, byte[] content)
		{
			var path = Path.Combine(folderPath, $"{name}.pdf");
			File.WriteAllBytes(path, content);
		}
	}
}