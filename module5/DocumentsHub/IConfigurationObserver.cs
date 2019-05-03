using System;
using System.ComponentModel;
using DocumentsJoiner.Configuration;

namespace DocumentsHub
{
	public interface IConfigurationObserver : INotifyPropertyChanged, IDisposable
	{
		DocumentsJoinerConfigurationSection DocumentsJoinerConfiguration { get; }
		void Start();
		void Stop();
	}
}