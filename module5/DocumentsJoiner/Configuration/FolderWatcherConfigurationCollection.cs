using System.Configuration;

namespace DocumentsJoiner.Configuration
{
	public class FolderWatcherConfigurationCollection : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new FolderWatcherConfigurationElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((FolderWatcherConfigurationElement) element).Path.GetHashCode();
		}
	}
}