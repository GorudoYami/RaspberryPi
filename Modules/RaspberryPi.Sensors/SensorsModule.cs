using Microsoft.Extensions.Logging;
using RaspberryPi.Common.Modules;

namespace RaspberryPi.Sensors;

public class SensorsModule : ISensorsModule {
	public bool IsInitialized { get; private set; }

	private readonly ILogger<ISensorsModule> _logger;

	public SensorsModule(ILogger<ISensorsModule> logger) {
		_logger = logger;
	}


	public Task InitializeAsync(CancellationToken cancellationToken = default) {
		IsInitialized = true;
		return Task.CompletedTask;
	}
}
