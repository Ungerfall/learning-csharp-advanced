using System.Text;

namespace DocumentsJoiner
{
	public static class DocumentsJoinerStatus
	{
		public const string CREATED = "Created";
		public const string PROCESSING_EXISTING_FILES = "Processing existing files";
		public const string WAITING = "Waiting files";
		public const string PROCESSING_FILES = "Processing files";
		public const string BUILDING_BATCH = "Building a batch";
		public const string STOPPED = "Stopped";

		public static Encoding Encoding = Encoding.UTF8;
	}
}