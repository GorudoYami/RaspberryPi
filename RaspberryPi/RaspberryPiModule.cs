using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Events;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Common.Services;
using RaspberryPi.Common.Utilities;
using RaspberryPi.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi {
	public class RaspberryPiModule : IRaspberryPiModule {
		public bool Enabled => true;

		private readonly RaspberryPiOptions _options;
		private readonly CancellationToken _cancellationToken;
		private readonly ILogger<IRaspberryPiModule> _logger;
		private readonly IEnumerable<IService> _services;
		private readonly ICommunicationProtocol _communicationProtocol;
		private readonly ISensorService _sensorService;
		private readonly IDrivingService _drivingService;
		private readonly ICameraService _cameraService;
		private readonly ITcpServerService _tcpServerService;

		public RaspberryPiModule(
			IOptions<RaspberryPiOptions> options,
			ILogger<IRaspberryPiModule> logger,
			IEnumerable<IService> services,
			ICancellationTokenProvider cancellationTokenProvider,
			ICommunicationProtocol communicationProtocol,
			ISensorService sensorService,
			IDrivingService drivingService,
			ICameraService cameraService,
			ITcpServerService tcpServerService) {
			_logger = logger;
			_options = options.Value;
			_cancellationToken = cancellationTokenProvider.GetToken();
			_services = services;
			_communicationProtocol = communicationProtocol;
			_sensorService = sensorService;
			_cameraService = cameraService;
			_tcpServerService = tcpServerService;
			_drivingService = drivingService;
		}

		public async Task InitializeAsync(CancellationToken cancellationToken = default) {
			try {
				_logger.LogDebug("Found {ServiceCount} services.", _services.Count());
				_logger.LogDebug("Initializing services...");

				var initializableServices = _services.Where(x => x.Enabled && x is IInitializableService).Cast<IInitializableService>();
				foreach (IInitializableService service in initializableServices) {
					await service.InitializeAsync(cancellationToken);
				}
				_logger.LogDebug("Services initialized: {ServicesInitializedCount}.", initializableServices.Count());

				if (_options.DefaultSafety) {
					_logger.LogDebug("Enabling safety by default");
					_sensorService.SensorTriggered += OnSensorTriggered;
					_sensorService.Start();
				}

				_communicationProtocol.MessageReceived += OnMessageReceived;
				_logger.LogDebug("Initialization completed.");
			}
			catch (Exception ex) {
				_logger.LogCritical(ex, "Caught error during initialization");
			}
		}

		private void OnSensorTriggered(object sender, SensorTriggeredEventArgs e) {
			_drivingService.Stop();
		}

		private void OnMessageReceived(object sender, MessageReceivedEventArgs e) {
			_logger.LogDebug("Message received: {MessageType} with value {MessageValue}", e.Type.ToString(), e.Value);

			switch (e.Type) {
				case MessageType.DriveForward:
					_drivingService.Forward(e.Value / 255d);
					break;
				case MessageType.DriveBackward:
					_drivingService.Backward(e.Value / 255d);
					break;
				case MessageType.DriveLeft:
					_drivingService.Left(e.Value / 255d);
					break;
				case MessageType.DriveRight:
					_drivingService.Right(e.Value / 2555d);
					break;
				case MessageType.DriveStraight:
					_drivingService.Straight();
					break;
				case MessageType.DriveStop:
					_drivingService.Stop();
					break;
				case MessageType.SensorsDisable:
					_sensorService.SensorTriggered -= OnSensorTriggered;
					break;
				case MessageType.SensorsEnable:
					_sensorService.SensorTriggered += OnSensorTriggered;
					break;
				case MessageType.CameraEnable:
					_cameraService.Start();
					break;
				case MessageType.CameraDisable:
					_cameraService.Stop();
					break;
				case MessageType.Unknown:
					_logger.LogError("Unknown message type received with value ({MessageValue})", e.Value);
					break;
			}
		}

		public async Task RunAsync() {
			await InitializeAsync();

			try {
				_tcpServerService.Start();
			}
			catch (Exception ex) {
				_logger.LogCritical(ex, "Caught error during server startup");
			}

			while (_cancellationToken.IsCancellationRequested == false) {
				await ReconnectModulesAsync();
				Thread.Sleep(250);
			}
		}

		private async Task ReconnectModulesAsync() {
			await Task.WhenAll(_services
				.OfType<IConnectableService>()
				.Where(x => x.Connected == false)
				.Select(x => {
					try {
						return x.ConnectAsync(_cancellationToken);
					}
					catch (Exception ex) {
						_logger.LogWarning(ex, "Could not reconnect module");
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
}
