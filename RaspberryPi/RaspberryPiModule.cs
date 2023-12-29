using System.Net.NetworkInformation;
using Microsoft.Extensions.Logging;
using GorudoYami.Common.Modules;
using RaspberryPi.Common.Modules;
using Microsoft.Extensions.Options;
using RaspberryPi.Models;
using System.Net;
using GorudoYami.Common.Asynchronous;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Common.Events;
using RaspberryPi.Common.Modules.Providers;

namespace RaspberryPi;

public class RaspberryPiModule : IRaspberryPiModule {
	public bool LazyInitialization => false;
	public bool IsInitialized { get; private set; }

	private readonly RaspberryPiModuleOptions _options;
	private readonly ILogger<IRaspberryPiModule> _logger;
	private readonly IClientProtocol _protocol;
	private readonly ICollection<IModule> _modules;
	private readonly IClientModule _clientModule;
	private readonly IDrivingModule _drivingModule;
	private readonly IModemModule _modemModule;
	private readonly ICameraModule _cameraModule;
	private readonly ISensorsModule _sensorsModule;
	private readonly CancellationToken _cancellationToken;

	public RaspberryPiModule(
		IOptions<RaspberryPiModuleOptions> options,
		ILogger<IRaspberryPiModule> logger,
		ICancellationTokenProvider cancellationTokenProvider,
		IClientProtocol protocol,
		IClientModule clientModule,
		IDrivingModule drivingModule,
		IModemModule modemModule,
		ICameraModule cameraModule,
		ISensorsModule sensorsModule) {
		_options = options.Value;
		_logger = logger;
		_cancellationToken = cancellationTokenProvider.GetToken();
		_protocol = protocol;
		_modemModule = modemModule;
		_drivingModule = drivingModule;
		_cameraModule = cameraModule;
		_sensorsModule = sensorsModule;
		_clientModule = clientModule;

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

		if (_options.DefaultSafety) {
			_sensorsModule.SensorTriggered += OnSensorTriggered;
		}

		_protocol.MessageReceived += OnMessageReceived;
		_logger.LogDebug("Modules initialized: {ModulesInitializedCount}", _modules.Count(x => x.IsInitialized));
	}

	private void OnSensorTriggered(object? sender, SensorTriggeredEventArgs e) {
		_drivingModule.Stop();
	}

	private void OnMessageReceived(object? sender, MessageReceivedEventArgs e) {
		switch (e.Type) {
			case MessageType.DriveForward:
				_drivingModule.Forward(e.Value / 255d);
				break;
			case MessageType.DriveBackward:
				_drivingModule.Backward(e.Value / 255d);
				break;
			case MessageType.DriveLeft:
				_drivingModule.Left(e.Value / 255d);
				break;
			case MessageType.DriveRight:
				_drivingModule.Right(e.Value / 2555d);
				break;
			case MessageType.DriveStraight:
				_drivingModule.Straight();
				break;
			case MessageType.DriveStop:
				_drivingModule.Stop();
				break;
			case MessageType.SensorsDisable:
				_sensorsModule.SensorTriggered -= OnSensorTriggered;
				break;
			case MessageType.SensorsEnable:
				_sensorsModule.SensorTriggered += OnSensorTriggered;
				break;
			case MessageType.CameraEnable:
				_cameraModule.Start();
				break;
			case MessageType.CameraDisable:
				_cameraModule.Stop();
				break;
			case MessageType.Unknown:
				_logger.LogError("Unknown message type received ({MessageType}) with value ({MessageValue})", (byte)e.Type, e.Value);
				break;
		}
	}

	public async Task RunAsync() {
		await InitializeAsync();

		while (_cancellationToken.IsCancellationRequested == false) {
			try {
				await ReconnectNetworkModules();
			}
			catch (Exception ex) {
				_logger.LogCritical(ex, "Caught error in main loop.");
			}
		}
	}

	private async Task ReconnectNetworkModules(CancellationToken cancellationToken = default) {
		foreach (INetworkingProvider module in _modules.OfType<INetworkingProvider>().Where(x => x.Connected == false)) {
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
