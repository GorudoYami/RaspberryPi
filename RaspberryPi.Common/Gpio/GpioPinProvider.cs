using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;

namespace RaspberryPi.Common.Gpio {
	public class GpioPinProvider : IGpioPinProvider {
		private readonly GpioPin _pin;

		public GpioPinProvider(GpioPin pin) {
			_pin = pin;
		}
	}
}
