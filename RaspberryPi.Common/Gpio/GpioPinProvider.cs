using System.Device.Gpio;

namespace RaspberryPi.Common.Gpio;
public class GpioPinProvider : IGpioPinProvider {
	private readonly GpioPin _pin;

	public GpioPinProvider(GpioPin pin) {
		_pin = pin;
	}
}
