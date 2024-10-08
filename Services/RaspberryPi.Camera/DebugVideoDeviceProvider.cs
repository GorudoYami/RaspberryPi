﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Camera.Options;
using RaspberryPi.Common.Events;
using RaspberryPi.Common.Providers;
using System;
using System.Timers;
using Timer = System.Timers.Timer;

namespace RaspberryPi.Camera {
	public class DebugVideoDeviceProvider : IVideoDeviceProvider, IDisposable {
		public event EventHandler<VideoDeviceImageCapturedEventArgs> ImageCaptured;

		private readonly Timer _timer;
		private readonly Random _random;
		private readonly object _lock;
		private readonly byte[] _buffer;
		private readonly ILogger<IVideoDeviceProvider> _logger;

		public DebugVideoDeviceProvider(ILogger<IVideoDeviceProvider> logger, IOptions<VideoDeviceOptions> options) {
			_logger = logger;
			_timer = new Timer(1000);
			_timer.Elapsed += SendImageCaptured;
			_lock = new object();
			_buffer = new byte[options.Value.Width * options.Value.Height];
			_random = new Random();
		}

		event EventHandler<VideoDeviceImageCapturedEventArgs> IVideoDeviceProvider.ImageCaptured {
			add => throw new NotImplementedException();

			remove => throw new NotImplementedException();
		}

		private void SendImageCaptured(object sender, ElapsedEventArgs e) {
			lock (_lock) {
				_random.NextBytes(_buffer);
				_logger.LogDebug("Simulated capture of {BufferLength} bytes as image", _buffer.Length);
				ImageCaptured?.Invoke(this, new VideoDeviceImageCapturedEventArgs(_buffer, _buffer.Length));
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
