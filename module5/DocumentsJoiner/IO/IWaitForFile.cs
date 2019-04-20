using System.IO;

namespace DocumentsJoiner.IO
{
	public interface IWaitForFile
	{
		FileStream AttemptToReadFile(string fullPath);
	}
}