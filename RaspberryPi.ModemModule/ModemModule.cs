using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Modules;
using RaspberryPi.Modules.Models;

namespace RaspberryPi.Modules;

public class ModemModule : IModemModule {
	private readonly ILogger<IModemModule> _logger;
	private readonly ModemModuleOptions _options;

	public ModemModule(IOptions<ModemModuleOptions> options, ILogger<IModemModule> logger) {
		_logger = logger;
		_options = options.Value;
	}
}
