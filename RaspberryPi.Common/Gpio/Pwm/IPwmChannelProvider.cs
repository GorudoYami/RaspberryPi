namespace RaspberryPi.Common.Gpio.Pwm {
	public interface IPwmChannelProvider {
		int Frequency { get; set; }
		double DutyCycle { get; set; }

		void Start();
		void Stop();
	}
}
