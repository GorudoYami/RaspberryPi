using System;

namespace RaspberryPi.Common.Protocols;
public interface IServerProtocol : IProtocol {
	event EventHandler<MessageReceivedEventArgs> MessageReceived;

	void ParseMessage(byte[] message);
}
