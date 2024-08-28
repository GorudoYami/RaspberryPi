using System;

namespace RaspberryPi.Common.Protocols;
public class MessageReceivedEventArgs : EventArgs {
	public MessageType Type { get; }
	public byte Value { get; }

	public MessageReceivedEventArgs(MessageType type, byte value) {
		Type = type;
		Value = value;
	}
}
