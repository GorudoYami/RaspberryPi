using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaspberryPi.Drivers {
	public interface IDriverPin {
		public int Key { get; set; }
		public PinMode Mode { get; set; }
		public bool Open { get; set; }
	}
}