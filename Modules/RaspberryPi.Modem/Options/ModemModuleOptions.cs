using RaspberryPi.Modem.Models;
using System.Collections.Generic;

namespace RaspberryPi.Modem.Options {
	public class ModemModuleOptions {
		public string SerialPort { get; set; }
		public int DefaultBaudRate { get; set; }
		public int TargetBaudRate { get; set; }
		public int ServerPort { get; set; }
		public int TimeoutSeconds { get; set; }
		public ICollection<ExpectedResponse> ExpectedResponses { get; set; }

		public static bool Validate(ModemModuleOptions options) {
			return true; //zzz
		}
	}
}
