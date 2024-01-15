using System;

namespace RaspberryPi.Client.Options {
	public class ClientModuleOptions {
		public string ServerHost { get; set; }
		public int ServerPort { get; set; }
		public int TimeoutSeconds { get; set; }

		public static bool Validate(ClientModuleOptions options) {
			throw new NotImplementedException();
		}
	}
}
