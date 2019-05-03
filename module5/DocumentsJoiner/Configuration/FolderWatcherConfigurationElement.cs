using System;
using System.Configuration;

namespace DocumentsJoiner.Configuration
{
	public class FolderWatcherConfigurationElement : ConfigurationElement, IEquatable<FolderWatcherConfigurationElement>
	{
		public int Key => Path.GetHashCode();

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

		public bool Equals(FolderWatcherConfigurationElement other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;

			return Path.Equals(other.Path)
				&& Filter.Equals(other.Filter);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((FolderWatcherConfigurationElement) obj);
		}

		public override int GetHashCode()
		{
			return Path.GetHashCode() ^ Filter.GetHashCode();
		}
	}
}