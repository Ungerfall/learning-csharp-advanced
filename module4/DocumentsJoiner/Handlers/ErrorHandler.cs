﻿using System;
using System.IO;

namespace DocumentsJoiner.Handlers
{
	public class ErrorHandler : IDocumentErrorHandler
	{
		private readonly string brokenFilesDirectoryFullPath;

		public ErrorHandler(string brokenFilesDirectoryFullPath)
		{
			this.brokenFilesDirectoryFullPath = brokenFilesDirectoryFullPath;
		}

		public void Handle(Document document, Exception exception)
		{
			var path = Path.Combine(brokenFilesDirectoryFullPath, document.Name);
			using (var file = File.Create(path))
			{
				document.Stream.Seek(0L, SeekOrigin.Begin);
				document.Stream.CopyTo(file);
			}

			var errorDocumentName = document.Name + ".error";
			var errorPath = Path.Combine(brokenFilesDirectoryFullPath, errorDocumentName);
			File.WriteAllText(errorPath, exception.Message);
		}
	}
}