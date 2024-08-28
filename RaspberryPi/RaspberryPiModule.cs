using GorudoYami.Common.Asynchronous;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Events;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Common.Services;
using RaspberryPi.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi;

public class RaspberryPiModule(
	IOptions<RaspberryPiModuleOptions> options,
	ILogger<IRaspberryPiModule> logger,
	IEnumerable<IService> services,
	ICancellationTokenProvider cancellationTokenProvider,
	ICommunicationProtocol communicationProtocol,
	ISensorService sensorService,
	IDrivingService drivingService,
	ICameraService cameraService,
	ITcpServerService tcpServerService) : IRaspberryPiModule {
	public bool Enabled => true;

	private readonly RaspberryPiModuleOptions _options = options.Value;
	private readonly CancellationToken _cancellationToken = cancellationTokenProvider.GetToken();

	public async Task InitializeAsync(CancellationToken cancellationToken = default) {
		try {
			logger.LogDebug("Found {ServiceCount} services.", services.Count());
			logger.LogDebug("Initializing services...");

			var initializableServices = services.Where(x => x.Enabled && x is IInitializableService).Cast<IInitializableService>();
			foreach (IInitializableService service in initializableServices) {
				await service.InitializeAsync(cancellationToken);
			}
			logger.LogDebug("Services initialized: {ServicesInitializedCount}.", initializableServices.Count());

			if (_options.DefaultSafety) {
				logger.LogDebug("Enabling safety by default");
				sensorService.SensorTriggered += OnSensorTriggered;
			}

			communicationProtocol.MessageReceived += OnMessageReceived;
			logger.LogDebug("Initialization completed.");
		}
		catch (Exception ex) {
			logger.LogCritical(ex, "Caught error during initialization");
		}
	}

	private void OnSensorTriggered(object sender, SensorTriggeredEventArgs e) {
		drivingService.Stop();
	}

	private void OnMessageReceived(object sender, MessageReceivedEventArgs e) {
		logger.LogDebug("Message received: {MessageType} with value {MessageValue}", e.Type.ToString(), e.Value);

		switch (e.Type) {
			case MessageType.DriveForward:
				drivingService.Forward(e.Value / 255d);
				break;
			case MessageType.DriveBackward:
				drivingService.Backward(e.Value / 255d);
				break;
			case MessageType.DriveLeft:
				drivingService.Left(e.Value / 255d);
				break;
			case MessageType.DriveRight:
				drivingService.Right(e.Value / 2555d);
				break;
			case MessageType.DriveStraight:
				drivingService.Straight();
				break;
			case MessageType.DriveStop:
				drivingService.Stop();
				break;
			case MessageType.SensorsDisable:
				sensorService.SensorTriggered -= OnSensorTriggered;
				break;
			case MessageType.SensorsEnable:
				sensorService.SensorTriggered += OnSensorTriggered;
				break;
			case MessageType.CameraEnable:
				sensorService.Start();
				break;
			case MessageType.CameraDisable:
				cameraService.Stop();
				break;
			case MessageType.Unknown:
				logger.LogError("Unknown message type received ({MessageType}) with value ({MessageValue})", (byte)e.Type, e.Value);
				break;
		}
	}

	public async Task RunAsync() {
		await InitializeAsync();

		try {
			tcpServerService.Start();
		}
		catch (Exception ex) {
			logger.LogCritical(ex, "Caught error during server startup");
		}

		while (_cancellationToken.IsCancellationRequested == false) {
			await ReconnectModulesAsync();
			Thread.Sleep(250);
		}
	}

	private async Task ReconnectModulesAsync() {
		await Task.WhenAll(services
			.OfType<IConnectableService>()
			.Where(x => x.Connected == false)
			.Select(x => {
				try {
					return x.ConnectAsync(_cancellationToken);
				}
				catch (Exception ex) {
					logger.LogWarning(ex, "Could not reconnect module");
					return Task.CompletedTask;
				}
			}));
	}

	//private async Task<bool> PingAsync(IPAddress hostAddress, CancellationToken cancellationToken = default) {
	//	try {
	//		var ping = new Ping();
	//		PingReply reply = await ping.SendPingAsync(hostAddress, _options.PingTimeoutSeconds * 1000);
	//		return reply.Status == IPStatus.Success;
	//	}
	//	catch (Exception ex) {
	//		_logger.LogWarning(ex, "Pinging host {Host} failed", hostAddress.ToString());
	//		return false;
	//	}
	//}
}
