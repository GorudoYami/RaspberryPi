using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaspberryPi.Modules.Models;

public class ModemPin(int number, ModemPinType type) {
	public int Number { get; init; } = number;
	public ModemPinType Type { get; init; } = type;
}
