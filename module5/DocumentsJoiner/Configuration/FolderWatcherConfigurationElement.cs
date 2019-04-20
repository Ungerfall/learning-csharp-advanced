using System.Configuration;

namespace DocumentsJoiner.Configuration
{
	public class FolderWatcherConfigurationElement : ConfigurationElement
	{
		[ConfigurationProperty("path")]
		public string Path
		{
			get => (string) this["path"];
			set => this["path"] = value;
		}

		[ConfigurationProperty("filter")]
		public string Filter
		{
			get => (string) this["filter"];
			set => this["filter"] = value;
		}
	}
}