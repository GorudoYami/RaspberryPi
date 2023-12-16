using RaspberryPi.Drivers.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaspberryPi.CarModule.Models;

public interface IDriverPin {
	public int Key { get; set; }
	public PinMode Mode { get; set; }
	public bool Open { get; set; }
	public Direction BoundDirection { get; set; }
}