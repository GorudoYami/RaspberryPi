using System;

namespace RaspberryPi.Common.Events {
	public class VideoDeviceImageCapturedEventArgs : EventArgs {
		public byte[] Buffer { get; }
		public int Length { get; }

		public VideoDeviceImageCapturedEventArgs(byte[] buffer, int length) {
			Buffer = buffer;
			Length = length;
		}
	}
}
