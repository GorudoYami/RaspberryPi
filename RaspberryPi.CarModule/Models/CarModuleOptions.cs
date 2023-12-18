using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaspberryPi.CarModule.Models;
public class CarModuleOptions {
	public ICollection<IDriverPin> Pins { get; }

	public CarModuleOptions(ICollection<IDriverPin> pins) {
		Pins = pins;
	}
}
