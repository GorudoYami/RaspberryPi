using GorudoYami.Common.Asynchronous;
using GorudoYami.Common.Modules;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Events;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi {
	public class RaspberryPiModule : IRaspberryPiModule {
		public bool Enabled => true;
		public bool IsInitialized { get; private set; }

		private readonly RaspberryPiModuleOptions _options;
		private readonly ILogger<IRaspberryPiModule> _logger;
		private readonly CancellationToken _cancellationToken;
		private readonly IServerProtocol _serverProtocol;
		private readonly IDrivingModule _drivingModule;
		private readonly ICameraModule _cameraModule;
		private readonly ISensorsModule _sensorsModule;
		private readonly IServerModule _serverModule;
		private readonly ICollection<IModule> _modules;

		public RaspberryPiModule(
			IOptions<RaspberryPiModuleOptions> options,
			ILogger<IRaspberryPiModule> logger,
			ICancellationTokenProvider cancellationTokenProvider,
			IServerProtocol serverProtocol,
			IDrivingModule drivingModule,
			ICameraModule cameraModule,
			ISensorsModule sensorsModule,
			IServerModule serverModule) {
			_options = options.Value;
			_logger = logger;
			_cancellationToken = cancellationTokenProvider.GetToken();
			_serverProtocol = serverProtocol;
			_drivingModule = drivingModule;
			_cameraModule = cameraModule;
			_sensorsModule = sensorsModule;
			_serverModule = serverModule;

			_modules = new List<IModule>() {
				_drivingModule,
				_cameraModule,
				_sensorsModule,
				_serverModule
			};
		}

		public async Task InitializeAsync(CancellationToken cancellationToken = default) {
			try {
				_logger.LogDebug("Found {ModulesCount} modules", _modules.Count);
				_logger.LogDebug("Initializing modules...");

				foreach (IModule module in _modules.Where(x => x.IsInitialized == false && x.Enabled)) {
					await module.InitializeAsync(cancellationToken);
				}

				if (_options.DefaultSafety) {
					_sensorsModule.SensorTriggered += OnSensorTriggered;
				}

				_serverProtocol.MessageReceived += OnMessageReceived;
				_logger.LogDebug("Modules initialized: {ModulesInitializedCount}", _modules.Count(x => x.IsInitialized));
			}
			catch (Exception ex) {
				_logger.LogCritical(ex, "Caught error during initialization");
			}
		}

		private void OnSensorTriggered(object sender, SensorTriggeredEventArgs e) {
			_drivingModule.Stop();
		}

		private void OnMessageReceived(object sender, MessageReceivedEventArgs e) {
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

			try {
				_serverModule.Start();
			}
			catch (Exception ex) {
				_logger.LogCritical(ex, "Caught error during server startup");
			}

			while (_cancellationToken.IsCancellationRequested == false) {
				try {
					Thread.Sleep(10000);
					ReportStatus();
				}
				catch (Exception ex) {
					_logger.LogCritical(ex, "Caught error in main loop.");
				}
			}
		}

		private void ReportStatus() {
			_logger.LogInformation("Status report");
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
