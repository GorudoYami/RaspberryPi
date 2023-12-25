using Microsoft.Extensions.Logging;
using RaspberryPi.Common.Modules;

namespace RaspberryPi.Sensors;

public class SensorsModule : ISensorsModule {
	private readonly ILogger<ISensorsModule> _logger;

	public SensorsModule(ILogger<ISensorsModule> logger) {
		_logger = logger;
	}
}
