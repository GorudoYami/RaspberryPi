using GorudoYami.Common.Modules;
using Iot.Device.Media;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Camera.Models;
using RaspberryPi.Common.Modules;

namespace RaspberryPi.Camera;

public class CameraModule : ICameraModule, IDisposable {
	public bool LazyInitialization => false;
	public bool IsInitialized { get; private set; }
	public MemoryStream VideoStream { get; init; }

	private readonly CameraModuleOptions _options;
	private VideoDevice? _videoDevice;

	public CameraModule(IOptions<CameraModuleOptions> options) {
		_options = options.Value;
		VideoStream = new MemoryStream();
	}

	public Task InitializeAsync(CancellationToken cancellationToken = default) {
		return Task.Run(() => {
			var videoSettings = new VideoConnectionSettings(0, (_options.Width, _options.Height), _options.Format);
			_videoDevice = VideoDevice.Create(videoSettings);
			if (_videoDevice.IsOpen == false) {
				throw new InitializeModuleException("Could not open video device");
			}
			IsInitialized = true;
		}, cancellationToken);
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
