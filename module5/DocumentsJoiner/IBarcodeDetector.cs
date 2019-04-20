using System.IO;

namespace DocumentsJoiner
{
	public interface IBarcodeDetector
	{
		string GetBarcode(Stream imageStream);
	}
}