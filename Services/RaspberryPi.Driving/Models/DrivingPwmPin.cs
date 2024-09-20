using RaspberryPi.Driving.Enums;

namespace RaspberryPi.Driving.Models {
	public class DrivingPwmPin(int channel, DrivingPinType type, int chip, int frequency) {
		public int Chip { get; } = chip;
		public int Frequency { get; } = frequency;
		public int Channel { get; } = channel;
		public DrivingPinType Type { get; } = type;
	}
}
