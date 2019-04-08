using System.Configuration;

namespace DocumentsJoiner.Configuration
{
	public class DocumentsJoinerConfigurationSection : ConfigurationSection
	{
		[ConfigurationProperty("broken-files-folder", IsRequired = true, IsKey = false)]
		public string BrokenFilesDirectory
		{
			get => (string)this["broken-files-folder"];
			set => this["broken-files-folder"] = value;
		}

		[ConfigurationProperty("timeout", IsRequired = false, DefaultValue = "60000", IsKey = false)]
		public int Timeout
		{
			get => (int) this["timeout"];
			set => this["timeout"] = value;
		}

		[ConfigurationProperty("barcode", IsRequired = true, IsKey = false)]
		public string BarcodeSeparatorValue
		{
			get => (string) this["barcode"];
			set => this["barcode"] = value;
		}

		[ConfigurationProperty("attempts", IsRequired = true, DefaultValue = "60000", IsKey = false)]
		public int AttemptsToOpenFile
		{
			get => (int) this["attempts"];
			set => this["attempts"] = value;
		}

		[ConfigurationProperty("batches", IsRequired = true, IsKey = false)]
		public string BatchesFolder
		{
			get => (string) this["batches"];
			set => this["batches"] = value;
		}

		[ConfigurationProperty("watchers", IsDefaultCollection = false)]
		[ConfigurationCollection(
			typeof(FolderWatcherConfigurationCollection),
			AddItemName = "add",
			ClearItemsName = "clear",
			RemoveItemName = "remove")]
		public FolderWatcherConfigurationCollection Watchers
		{
			get => (FolderWatcherConfigurationCollection)base["watchers"];
			set => base["watchers"] = value;
		}
	}
}