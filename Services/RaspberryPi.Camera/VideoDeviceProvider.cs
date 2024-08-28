using Iot.Device.Media;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Events;
using RaspberryPi.Common.Options;
using RaspberryPi.Common.Providers;
using System;

namespace RaspberryPi.Camera {
	public class VideoDeviceProvider : IVideoDeviceProvider, IDisposable {
		public event EventHandler<VideoDeviceImageCapturedEventArgs> ImageCaptured;

		private readonly VideoDevice _videoDevice;

		public VideoDeviceProvider(IOptions<VideoDeviceOptions> options) {
			var videoSettings = new VideoConnectionSettings(0, (options.Value.Width, options.Value.Height), options.Value.Format);
			_videoDevice = VideoDevice.Create(videoSettings);
			_videoDevice.NewImageBufferReady += OnNewImageBufferReady;
		}

		private void OnNewImageBufferReady(object sender, NewImageBufferReadyEventArgs e) {
			ImageCaptured?.Invoke(sender, new VideoDeviceImageCapturedEventArgs() {
				Buffer = e.ImageBuffer,
				Length = e.Length
			});
		}

		public void StartCaptureContinuous() =>
			_videoDevice.StartCaptureContinuous();

		public void StopCaptureContinuous() =>
			_videoDevice.StopCaptureContinuous();

		public void Dispose() {
			GC.SuppressFinalize(this);
			_videoDevice.NewImageBufferReady -= OnNewImageBufferReady;
			_videoDevice.Dispose();
		}
	}
}
