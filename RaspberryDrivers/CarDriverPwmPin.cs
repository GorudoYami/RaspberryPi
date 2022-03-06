using RaspberryPi.Drivers.Enums;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaspberryPi.Drivers {
	public class CarDriverPwmPin : IDriverPin {
		public int PwmChip { get; set; }
		public int Key { get; set; }
		public int Frequency { get; set; }
		public PinMode Mode { get; set; }
		public bool Open { get; set; }
		public Direction BoundDirection { get; set; }
	}
}
