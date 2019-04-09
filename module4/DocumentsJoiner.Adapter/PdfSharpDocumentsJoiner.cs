using System.Collections.Generic;
using System.IO;
using System.Threading;
using DocumentsJoiner.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace DocumentsJoiner.Wrappers
{
	public class PdfSharpDocumentsJoiner : IDocumentsJoiner
	{
		private readonly CancellationToken token;
		private readonly IWaitForFile fileReader;

		public PdfSharpDocumentsJoiner(CancellationToken token, IWaitForFile fileReader)
		{
			this.token = token;
			this.fileReader = fileReader;
		}

		public Stream Join(IEnumerable<Document> documents)
		{
			var documentStream = new MemoryStream();
			var outputDocument = new PdfDocument();
			foreach (var document in documents)
			{
				if (token.IsCancellationRequested)
				{
					break;
				}

				var page = new PdfPage(outputDocument);
				var context = XGraphics.FromPdfPage(page);
				using (var file = fileReader.AttemptToReadFile(document.FullPath))
				using (var image = XImage.FromStream(file))
				{
					context.DrawImage(image, new XPoint(0D, 0D));
					outputDocument.AddPage(page);
				}
			}

			if (outputDocument.PageCount > 0)
			{
				outputDocument.Save(documentStream, closeStream: false);
			}

			documentStream.Seek(0L, SeekOrigin.Begin);
			return documentStream;
		}
	}
}