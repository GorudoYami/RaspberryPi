using RaspberryPi.Common.Options;

namespace RaspberryPi.Server.Models {
	public class ServerModuleOptions : IModuleOptions {
		public bool Enabled { get; set; }
		public string Host { get; set; }
		public int MainPort { get; set; }
		public int VideoPort { get; set; }
	}
}
