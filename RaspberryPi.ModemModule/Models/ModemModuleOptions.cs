namespace RaspberryPi.Modules.Models;

public class ModemModuleOptions(ICollection<IPin> pins) {
	public required ICollection<IPin> Pins { get; init; } = pins;
}
