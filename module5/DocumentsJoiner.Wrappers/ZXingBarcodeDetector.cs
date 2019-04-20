using System.Drawing;
using System.IO;
using ZXing;

namespace DocumentsJoiner.Wrappers
{
	public class ZXingBarcodeDetector : IBarcodeDetector
	{
		private readonly BarcodeReader reader = new BarcodeReader();

		public string GetBarcode(Stream imageStream)
		{
			var bitmap = (Bitmap) Image.FromStream(imageStream);
			var result = reader.Decode(bitmap);

			return result?.Text;
		}
	}
}