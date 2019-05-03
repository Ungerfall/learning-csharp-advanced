using System.Configuration;

namespace DocumentsJoiner.Configuration
{
	public static class ConfigurationContext
	{
		public const string SERVICE_CONFIGURATION_SECTION = "DocumentsJoiner";

		private static volatile int timeout;
		private static volatile string barcodeSeparator;

		static ConfigurationContext()
		{
			var configuration
				= (DocumentsJoinerConfigurationSection) ConfigurationManager.GetSection(SERVICE_CONFIGURATION_SECTION);
			Timeout = configuration.Timeout;
			BarcodeSeparator = configuration.BarcodeSeparatorValue;
		}

		public static int Timeout
		{
			get => timeout;
			set => timeout = value;
		}

		public static string BarcodeSeparator
		{
			get => barcodeSeparator;
			set => barcodeSeparator = value;
		}
	}
}