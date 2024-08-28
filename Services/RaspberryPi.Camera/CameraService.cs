using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Camera.Options;
using RaspberryPi.Common.Events;
using RaspberryPi.Common.Providers;
using RaspberryPi.Common.Services;
using System;
using System.Linq;

namespace RaspberryPi.Camera;
public class CameraService : ICameraService {
	public bool Enabled => _options.Enabled;
	public bool IsInitialized { get; private set; }

	private readonly CameraServiceOptions _options;
	private readonly ILogger<ICameraService> _logger;
	private readonly ITcpServerService _tcpServerService;
	private readonly IVideoDeviceProvider _videoDeviceProvider;

	public CameraService(
		IOptions<CameraServiceOptions> options,
		ILogger<ICameraService> logger,
		ITcpServerService tcpServerService,
		IVideoDeviceProvider videoDeviceProvider) {
		_options = options.Value;
		_logger = logger;
		_tcpServerService = tcpServerService;
		_videoDeviceProvider = videoDeviceProvider;
		_videoDeviceProvider.ImageCaptured += OnImageCaptured;
	}

	private async void OnImageCaptured(object sender, VideoDeviceImageCapturedEventArgs e) {
		try {
			await _tcpServerService.BroadcastAsync(e.Buffer.Take(e.Length).ToArray());
		}
		catch (Exception ex) {
			_logger.LogError(ex, "Error occured during video stream. Stopping capturing.");
			Stop();
		}
	}

	public void Start() {
		_videoDeviceProvider.StartCaptureContinuous();
		// Might need to call _videoDevice.CaptureContinous() in separate thread?
	}

	public void Stop() {
		_videoDeviceProvider.StopCaptureContinuous();
	}
}
