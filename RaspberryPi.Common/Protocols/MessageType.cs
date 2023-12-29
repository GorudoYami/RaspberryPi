namespace RaspberryPi.Common.Protocols;

public enum MessageType : byte {
	Unknown = 0,
	DriveForward = 1,
	DriveBackward = 2,
	DriveStraight = 3,
	DriveLeft = 4,
	DriveRight = 5,
	DriveStop = 6,
	SensorsEnable = 7,
	SensorsDisable = 8,
	CameraEnable = 9,
	CameraDisable = 10,
}
