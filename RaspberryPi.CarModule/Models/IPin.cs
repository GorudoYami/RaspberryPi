﻿
using RaspberryPi.Modules.Enums;
using System.Device.Gpio;

namespace RaspberryPi.Modules.Models;

public interface IPin {
	int Number { get; init; }
	PinMode Mode { get; init; }
	Direction Direction { get; init; }
}