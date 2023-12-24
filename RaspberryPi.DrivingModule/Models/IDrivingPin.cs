
using RaspberryPi.Modules.Enums;
using System.Device.Gpio;

namespace RaspberryPi.Modules.Models;

public interface IDrivingPin {
	int Number { get; init; }
	Direction Direction { get; init; }
}