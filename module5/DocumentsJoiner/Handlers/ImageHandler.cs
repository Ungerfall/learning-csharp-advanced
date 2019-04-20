using System;

namespace DocumentsJoiner.Handlers
{
	public class ImageHandler : IDocumentHandler
	{
		private readonly DocumentsController controller;

		public ImageHandler(DocumentsController controller)
		{
			this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
		}

		public bool Handle(Document document)
		{
			var batch = controller.CurrentBatch;
			if (batch == null || batch.Prefix != document.Prefix || batch.MaxSequenceNumber + 1 != document.Number)
			{
				controller.InitializeNewBatch(document);
			}

			controller.AddDocumentToBatch(document);
			return true;
		}
	}
}