using RaspberryPi.Driving.Enums;

namespace RaspberryPi.Driving.Models {
	public class DrivingPwmPin {
		public int Chip { get; }
		public int Frequency { get; }
		public int Channel { get; }
		public DrivingPinType Type { get; }

		public DrivingPwmPin(int channel, DrivingPinType type, int chip, int frequency) {
			Chip = chip;
			Frequency = frequency;
			Channel = channel;
			Type = type;
		}
	}
}
