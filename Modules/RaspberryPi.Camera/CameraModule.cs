using GorudoYami.Common.Modules;
using Iot.Device.Media;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Camera.Options;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Modules.Providers;
using RaspberryPi.Common.Resolvers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Camera {
	public class CameraModule : ICameraModule, IDisposable {
		public bool Enabled => _options.Enabled;
		public bool IsInitialized { get; private set; }

		private readonly CameraModuleOptions _options;
		private readonly ILogger<ICameraModule> _logger;
		private readonly IServerModule _serverModule;
		private VideoDevice _videoDevice;

		public CameraModule(IOptions<CameraModuleOptions> options, ILogger<ICameraModule> logger, IServerModule serverModule) {
			_options = options.Value;
			_logger = logger;
			_serverModule = serverModule;
		}

		public Task InitializeAsync(CancellationToken cancellationToken = default) {
			return Task.Run(() => {
				var videoSettings = new VideoConnectionSettings(0, (_options.Width, _options.Height), _options.Format);
				_videoDevice = VideoDevice.Create(videoSettings);
				if (_videoDevice.IsOpen == false) {
					throw new InitializeModuleException("Could not open video device");
				}
				_videoDevice.NewImageBufferReady += OnNewImageBufferReady;
				IsInitialized = true;
			}, cancellationToken);
		}

		private async void OnNewImageBufferReady(object sender, NewImageBufferReadyEventArgs e) {
			try {
				await _serverModule.BroadcastVideoAsync(e.ImageBuffer);
			}
			catch (Exception ex) {
				_logger.LogError(ex, "Error occured during video stream. Stopping capturing.");
				Stop();
			}
		}

		public void Start() {
			if (_videoDevice == null) {
				throw new InitializeModuleException("Module was not initialized");
			}

			_videoDevice.StartCaptureContinuous();
			// Might need to call _videoDevice.CaptureContinous() in separate thread?
		}

		public void Stop() {
			if (_videoDevice == null) {
				throw new InitializeModuleException("Module was not initialized");
			}

			_videoDevice.StopCaptureContinuous();
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			_videoDevice?.Dispose();
		}
	}
}
