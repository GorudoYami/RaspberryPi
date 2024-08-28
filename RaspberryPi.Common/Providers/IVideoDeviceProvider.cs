using RaspberryPi.Common.Events;
using System;

namespace RaspberryPi.Common.Providers;
public interface IVideoDeviceProvider {
	event EventHandler<VideoDeviceImageCapturedEventArgs>? ImageCaptured;

	void StartCaptureContinuous();
	void StopCaptureContinuous();
}
