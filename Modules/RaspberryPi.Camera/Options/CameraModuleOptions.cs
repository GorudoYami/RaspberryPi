using Iot.Device.Media;
using RaspberryPi.Common.Options;

namespace RaspberryPi.Camera.Options {
	public class CameraModuleOptions : IModuleOptions {
		public bool Enabled { get; set; }
		public uint Width { get; set; }
		public uint Height { get; set; }
		public VideoPixelFormat Format { get; set; }

		public static bool Validate(CameraModuleOptions options) {
			return options.Width <= 1920 && options.Height <= 1080;
		}
	}
}
