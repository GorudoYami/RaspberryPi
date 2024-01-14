using Iot.Device.Media;

namespace RaspberryPi.Camera.Models {
	public class CameraModuleOptions {
		public required uint Width { get; init; }
		public required uint Height { get; init; }
		public required VideoPixelFormat Format { get; init; }

		public static bool Validate(CameraModuleOptions options) {
			return options.Width <= 1920 && options.Height <= 1080;
		}
	}
}
