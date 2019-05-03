using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace DocumentsJoiner.Configuration
{
	public class FolderWatcherConfigurationCollection :
		ConfigurationElementCollection,
		IEquatable<FolderWatcherConfigurationCollection>
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new FolderWatcherConfigurationElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((FolderWatcherConfigurationElement) element).Key;
		}

		public bool Equals(FolderWatcherConfigurationCollection other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;

			var otherDict = other
				.Cast<FolderWatcherConfigurationElement>()
				.ToDictionary(x => x.Key, e => e);
			var thisDict = this
				.Cast<FolderWatcherConfigurationElement>()
				.ToDictionary(x => x.Key, e => e);
			var otherKeySet = new HashSet<int>(otherDict.Keys);
			var thisKeySet = new HashSet<int>(thisDict.Keys);
			if (!thisKeySet.SetEquals(otherKeySet))
			{
				return false;
			}

			foreach (var element in thisDict)
			{
				if (!element.Value.Equals(otherDict[element.Key]))
				{
					return false;
				}
			}

			return true;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((FolderWatcherConfigurationCollection) obj);
		}

		public override int GetHashCode()
		{
			return this
				.Cast<FolderWatcherConfigurationElement>()
				.Aggregate(base.GetHashCode(), (i, x) => i ^ x.Key);
		}
	}
}