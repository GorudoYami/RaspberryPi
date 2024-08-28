using RaspberryPi.Modem.Enums;

namespace RaspberryPi.Modem.Models {
	public class ModemPin {
		public int Number { get; }
		public ModemPinType Type { get; }

		public ModemPin(int number, ModemPinType type) {
			Number = number;
			Type = type;
		}
	}
}
