namespace RaspberryPi.Common.Events;
public class VideoDeviceImageCapturedEventArgs {
	public byte[] Buffer { get; set; }
	public int Length { get; set; }
}
