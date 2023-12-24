namespace RaspberryPi.Modem.Models;

public class ModemModuleOptions(ICollection<ModemPin> pins) {
	public required ICollection<ModemPin> Pins { get; init; } = pins;
}
