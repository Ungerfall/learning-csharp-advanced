using System;

namespace DocumentsJoiner
{
	public class DocumentBatchEventArgs : EventArgs
	{
		public DocumentBatch DocumentBatch { get; set; }
	}
}