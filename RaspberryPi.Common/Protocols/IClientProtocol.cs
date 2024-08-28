using System;

namespace RaspberryPi.Common.Protocols;
public interface IClientProtocol : IProtocol {
	event EventHandler<MessageReceivedEventArgs> MessageReceived;

	void ParseMessage(byte[] message);
}
