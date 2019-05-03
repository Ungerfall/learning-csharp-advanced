using System;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Threading;
using DocumentsJoiner.Configuration;
using JetBrains.Annotations;

namespace DocumentsHub
{
	public class ConfigurationObserver : IConfigurationObserver
	{
		private const string DOCUMENTS_JOINER_CONFIGURATION = "DocumentsJoiner";
		private const double CONFIGURATION_UPDATE_PERIOD_MS = 10 * 1000D;

		private readonly Timer configurationUpdater;

		public ConfigurationObserver()
		{
			configurationUpdater = new Timer(UpdateConfiguration);
		}

		public void Start()
		{
			configurationUpdater.Change(TimeSpan.Zero, TimeSpan.FromMilliseconds(CONFIGURATION_UPDATE_PERIOD_MS));
		}

		public void Stop()
		{
			configurationUpdater.Change(Timeout.Infinite, Timeout.Infinite);
		}

		public DocumentsJoinerConfigurationSection DocumentsJoinerConfiguration { get; private set; }
			= (DocumentsJoinerConfigurationSection) ConfigurationManager.GetSection(DOCUMENTS_JOINER_CONFIGURATION);

		private void UpdateConfiguration(object state)
		{
			var prevCfg = DocumentsJoinerConfiguration;
			ConfigurationManager.RefreshSection(DOCUMENTS_JOINER_CONFIGURATION);
			var cfg = (DocumentsJoinerConfigurationSection) ConfigurationManager
				.GetSection(DOCUMENTS_JOINER_CONFIGURATION);
			if (prevCfg.Timeout != cfg.Timeout
				|| prevCfg.AttemptsToOpenFile != cfg.AttemptsToOpenFile
				|| prevCfg.BarcodeSeparatorValue != cfg.BarcodeSeparatorValue
				|| prevCfg.BatchesFolder != cfg.BatchesFolder
				|| prevCfg.BrokenFilesDirectory != cfg.BrokenFilesDirectory
				|| prevCfg.OpeningFilePeriodMs != cfg.OpeningFilePeriodMs
				|| !prevCfg.Watchers.Equals(cfg.Watchers))
			{
				DocumentsJoinerConfiguration = cfg;
				OnPropertyChanged(nameof(DocumentsJoinerConfiguration));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void Dispose()
		{
			configurationUpdater?.Dispose();
			if (PropertyChanged != null)
			{
				foreach (var @delegate in PropertyChanged.GetInvocationList())
				{
					if (@delegate is PropertyChangedEventHandler handler)
					{
						PropertyChanged -= handler;
					}
				}
			}
		}
	}
}