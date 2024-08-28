using RaspberryPi.Common.Options;

namespace RaspberryPi.ModemMqtt.Options {
	public class ModemMqttModuleOptions : IModuleOptions {
		public bool Enabled { get; set; }
		public string ServerHost { get; set; }
		public int ServerPort { get; set; }
		public int QosLevel { get; set; }

		public static bool Validate(ModemMqttModuleOptions options) {
			return true;
		}
	}
}
