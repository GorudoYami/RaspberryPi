namespace RaspberryPi.Common.Events;

public class VideoDeviceImageCapturedEventArgs(byte[] buffer, int length) : EventArgs {
	public byte[] Buffer { get; } = buffer;
	public int Length { get; } = length;
}
