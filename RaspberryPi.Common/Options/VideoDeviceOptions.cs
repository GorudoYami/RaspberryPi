using Iot.Device.Media;

namespace RaspberryPi.Common.Options;
public class VideoDeviceOptions {
	public uint Width { get; set; }
	public uint Height { get; set; }
	public VideoPixelFormat Format { get; set; }
}
