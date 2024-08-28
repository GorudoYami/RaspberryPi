using Iot.Device.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaspberryPi.Common.Options {
	public class VideoDeviceOptions {
		public uint Width { get; set; }
		public uint Height { get; set; }
		public VideoPixelFormat Format { get; set; }
	}
}
