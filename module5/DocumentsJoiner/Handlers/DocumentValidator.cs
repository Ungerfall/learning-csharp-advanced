using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DocumentsJoiner.Handlers
{
	public class DocumentValidator : IDocumentHandler
	{
		private readonly string brokenFilesDirectoryFullPath;
		private readonly string[] validExtensions = new[] {".png", ".jpg"};
		private readonly Regex documentFormat = new Regex("(\\w+)_(\\w+)", RegexOptions.Compiled);

		public DocumentValidator(string brokenFilesDirectoryFullPath)
		{
			this.brokenFilesDirectoryFullPath = brokenFilesDirectoryFullPath;
		}

		public bool Handle(Document document)
		{
			if (document == null)
			{
				return false;
			}

			var filename = Path.GetFileName(document.FullPath);
			if (filename == null)
			{
				return true;
			}

			var match = documentFormat.Match(filename);
			if (!match.Success)
			{
				MoveDocumentToBrokenFolder(document);
				return true;
			}

			try
			{
				var prefix = match.Groups[1].Value;
				var number = int.Parse(match.Groups[2].Value);
				document.Prefix = prefix;
				document.Number = number;
			}
			catch (Exception)
			{
				MoveDocumentToBrokenFolder(document);
				return true;
			}

			var ext = Path.GetExtension(document.FullPath);
			if (!validExtensions.Contains(ext))
			{
				MoveDocumentToBrokenFolder(document);
				return true;
			}

			return false;
		}

		private void MoveDocumentToBrokenFolder(Document document)
		{
			var path = Path.Combine(brokenFilesDirectoryFullPath, document.Name);
			using (var file = File.Create(path))
			{
				document.Stream.CopyTo(file);
			}
		}
	}
}