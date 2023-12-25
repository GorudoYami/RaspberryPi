using Iot.Device.Media;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Camera.Models;
using RaspberryPi.Common.Modules;

namespace RaspberryPi.Camera;

public class CameraModule : ICameraModule, IDisposable {
	public MemoryStream VideoStream { get; init; }

	private readonly ILogger<ICameraModule> _logger;
	private readonly VideoDevice _videoDevice;

	public CameraModule(IOptions<CameraModuleOptions> options, ILogger<ICameraModule> logger) {
		_logger = logger;
		var videoSettings = new VideoConnectionSettings(0, (options.Value.Width, options.Value.Height), options.Value.Format);
		_videoDevice = VideoDevice.Create(videoSettings);
		_logger.LogDebug("Camera connection status: {CameraConnectionStatus}", _videoDevice.IsOpen);
		VideoStream = new MemoryStream();
		_videoDevice.NewImageBufferReady += (s, e) => VideoStream.Write(e.ImageBuffer);
	}

	public void Start() {
		_videoDevice.StartCaptureContinuous();
		// Might need to call _videoDevice.CaptureContinous() in separate thread?
	}

	public void Stop() {
		_videoDevice.StopCaptureContinuous();
	}

	public void Dispose() {
		GC.SuppressFinalize(this);
		_videoDevice.Dispose();
	}
}
