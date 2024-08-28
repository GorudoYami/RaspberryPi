using RaspberryPi.Common.Options;

namespace RaspberryPi.Client.Options {
	public class ClientModuleOptions : IServiceOptions {
		public bool Enabled { get; set; }
		public string ServerHost { get; set; }
		public int MainServerPort { get; set; }
		public int TimeoutSeconds { get; set; }
		public int VideoServerPort { get; set; }
	}
}
