using RaspberryPi.Modem.Enums;

namespace RaspberryPi.Modem.Models {
	public class ModemPin(int number, ModemPinType type) {
		public int Number { get; init; } = number;
		public ModemPinType Type { get; init; } = type;
	}
}
