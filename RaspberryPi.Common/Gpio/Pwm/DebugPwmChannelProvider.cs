using System;

namespace RaspberryPi.Common.Gpio.Pwm {
	sealed internal class DebugPwmChannelProvider
		: IPwmChannelProvider {

		public int Frequency {
			get {
				return _frequency;
			}
			set {
				Console.WriteLine($"Setting {value} Hz on pwm channel {_channel} of chip {_chip}");
				_frequency = value;
			}
		}
		public double DutyCycle {
			get {
				return _dutyCycle;
			}
			set {
				Console.WriteLine($"Setting {value} duty cycle on pwm channel {_channel} of chip {_chip}");
				_dutyCycle = value;
			}
		}

		private readonly int _chip;
		private readonly int _channel;
		private double _dutyCycle;
		private int _frequency;

		public DebugPwmChannelProvider(int chip, int channel, int frequency, double dutyCyclePercentage) {
			_chip = chip;
			_channel = channel;
			_frequency = frequency;
			_dutyCycle = dutyCyclePercentage;
		}

		public void Start() {
			Console.WriteLine($"Pwm channel {_channel} on chip {_chip} started with {Frequency} HZ and {DutyCycle} duty cycle");
		}

		public void Stop() {
			Console.WriteLine($"Pwm channel {_channel} on chip {_chip} stopped");
		}
	}
}
