using GorudoYami.Common.Modules;
using Iot.Device.Media;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Camera.Models;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Modules.Providers;
using RaspberryPi.Common.Providers;

namespace RaspberryPi.Camera;

public class CameraModule : ICameraModule, IDisposable {
	public bool LazyInitialization => false;
	public bool IsInitialized { get; private set; }

	private INetworkingProvider? Networking => _networkingProvider.GetNetworking();
	private readonly CameraModuleOptions _options;
	private readonly ILogger<ICameraModule> _logger;
	private readonly INetworkingResolver _networkingProvider;
	private VideoDevice? _videoDevice;

	public CameraModule(IOptions<CameraModuleOptions> options, ILogger<ICameraModule> logger, INetworkingResolver networkingProvider) {
		_options = options.Value;
		_logger = logger;
		_networkingProvider = networkingProvider;
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

	private void OnNewImageBufferReady(object sender, NewImageBufferReadyEventArgs e) {
		try {
			if (Networking == null) {
				_logger.LogError("No connection for video stream. Stopping capturing.");
				Stop();
				return;
			}

			Networking.SendAsync(e.ImageBuffer).GetAwaiter().GetResult();
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
