using System.Configuration;

namespace Messaging.Configuration
{
	public class MessagingConfigurationSection : ConfigurationSection
	{
		[ConfigurationProperty("chunk-size", IsRequired = false, DefaultValue = "4096", IsKey = false)]
		public int ChunkSize
		{
			get => (int) this["chunk-size"];
			set => this["chunk-size"] = value;
		}

		[ConfigurationProperty("host-name", IsRequired = false, DefaultValue = "localhost")]
		public string HostName
		{
			get => (string) this["host-name"];
			set => this["host-name"] = value;
		}

		[ConfigurationProperty("port", IsRequired = false, DefaultValue = "5672")]
		public int Port
		{
			get => (int) this["port"];
			set => this["port"] = value;
		}

		[ConfigurationProperty("user", IsRequired = true)]
		public string UserName
		{
			get => (string) this["user"];
			set => this["user"] = value;
		}

		[ConfigurationProperty("password", IsRequired = true)]
		public string Password
		{
			get => (string) this["password"];
			set => this["password"] = value;
		}

		[ConfigurationProperty("data-queue", IsRequired = true)]
		public string DataQueue
		{
			get => (string) this["data-queue"];
			set => this["data-queue"] = value;
		}
	}
}