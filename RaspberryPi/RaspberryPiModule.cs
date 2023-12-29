using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using GorudoYami.Common.Modules;
using RaspberryPi.Common.Modules;
using Microsoft.Extensions.Options;
using RaspberryPi.Models;
using System.Net;

namespace RaspberryPi;

public class RaspberryPiModule : IRaspberryPiModule {
	public bool LazyInitialization => false;
	public bool IsInitialized { get; private set; }

	private readonly RaspberryPiModuleOptions _options;
	private readonly ILogger<IRaspberryPiModule> _logger;
	private readonly ICollection<IModule> _modules;
	private readonly IClientModule _clientModule;
	private readonly IDrivingModule _drivingModule;
	private readonly IModemModule _modemModule;
	private readonly ICameraModule _cameraModule;
	private readonly ISensorsModule _sensorsModule;

	public RaspberryPiModule(
		IOptions<RaspberryPiModuleOptions> options,
		ILogger<IRaspberryPiModule> logger,
		IClientModule clientModule,
		IDrivingModule drivingModule,
		IModemModule modemModule,
		ICameraModule cameraModule,
		ISensorsModule sensorsModule) {
		_logger = logger;
		_modemModule = modemModule;
		_drivingModule = drivingModule;
		_cameraModule = cameraModule;
		_sensorsModule = sensorsModule;
		_clientModule = clientModule;
		_options = options.Value;

		_modules = new List<IModule>() {
			_modemModule,
			_drivingModule,
			_cameraModule,
			_sensorsModule,
			_clientModule
		};
	}

	public async Task InitializeAsync(CancellationToken cancellationToken = default) {
		_logger.LogDebug("Found {ModulesCount} modules", _modules.Count);
		_logger.LogDebug("Initializing modules...");
		foreach (IModule module in _modules.Where(x => x.IsInitialized != false && x.LazyInitialization == false)) {
			await module.InitializeAsync(cancellationToken);
		}
		_logger.LogDebug("Modules initialized: {ModulesInitializedCount}", _modules.Count(x => x.IsInitialized));
	}

	public async Task RunAsync() {
		await InitializeAsync();

		while (true) {
			await ReconnectNetworkModules();
		}
	}

	private async Task ReconnectNetworkModules(CancellationToken cancellationToken = default) {
		foreach (INetworkModule module in _modules.OfType<INetworkModule>().Where(x => x.Connected == false)) {
			try {
				if (module.IsInitialized == false) {
					await module.InitializeAsync(cancellationToken);
				}

				if (await PingAsync(module.ServerAddress, cancellationToken)) {
					await module.ConnectAsync(cancellationToken);
				}
			}
			catch (Exception ex) {
				_logger.LogError(ex, "Network module encountered an error and could not reconnect.");
			}
		}
	}

	private async Task<bool> PingAsync(IPAddress hostAddress, CancellationToken cancellationToken = default) {
		try {
			var ping = new Ping();
			PingReply reply = await ping.SendPingAsync(hostAddress, _options.PingTimeoutSeconds * 1000);
			return reply.Status == IPStatus.Success;
		}
		catch (Exception ex) {
			_logger.LogWarning(ex, "Pinging host {Host} failed", hostAddress.ToString());
			return false;
		}
	}
}
