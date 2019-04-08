using System;

namespace DocumentsJoiner.Handlers
{
	public class BarCodeHandler : IDocumentHandler
	{
		private readonly IBarcodeDetector detector;
		private readonly DocumentsController controller;

		public BarCodeHandler(IBarcodeDetector detector, DocumentsController controller)
		{
			this.detector = detector ?? throw new ArgumentNullException(nameof(detector));
			this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
		}

		public bool Handle(Document document)
		{
			var barcode = detector.GetBarcode(document.Stream);
			if (string.IsNullOrWhiteSpace(barcode))
			{
				return false;
			}

			controller.InitializeNewBatch(document);
			controller.AddDocumentToBatch(document);
			return true;
		}
	}
}