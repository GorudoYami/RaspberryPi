namespace RaspberryPi.Common.Protocols;

public class MessageReceivedEventArgs(MessageType type, byte value) : EventArgs {
	public MessageType Type { get; } = type;
	public byte Value { get; } = value;
}
