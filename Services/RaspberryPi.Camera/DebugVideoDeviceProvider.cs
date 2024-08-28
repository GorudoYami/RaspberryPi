using Microsoft.Extensions.Options;
using RaspberryPi.Common.Events;
using RaspberryPi.Common.Options;
using RaspberryPi.Common.Providers;
using System;
using System.Timers;

namespace RaspberryPi.Camera {
	public class DebugVideoDeviceProvider : IVideoDeviceProvider, IDisposable {
		public event EventHandler<VideoDeviceImageCapturedEventArgs> ImageCaptured;

		private readonly Timer _timer;
		private readonly Random _random;
		private readonly object _lock;
		private readonly byte[] _buffer;

		public DebugVideoDeviceProvider(IOptions<VideoDeviceOptions> options) {
			_timer = new Timer();
			_timer.Elapsed += SendImageCaptured;
			_lock = new object();
			_buffer = new byte[options.Value.Width * options.Value.Height];
			_random = new Random();
		}

		private void SendImageCaptured(object sender, ElapsedEventArgs e) {
			lock (_lock) {
				_random.NextBytes(_buffer);
				ImageCaptured?.Invoke(this, new VideoDeviceImageCapturedEventArgs() {
					Buffer = _buffer,
					Length = _buffer.Length
				});
			}
		}

		public void StartCaptureContinuous() {
			_timer.Start();
		}

		public void StopCaptureContinuous() {
			_timer.Stop();
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			if (_timer.Enabled) {
				_timer.Stop();
			}

			lock (_lock) {
				_timer.Dispose();
			}
		}
	}
}
