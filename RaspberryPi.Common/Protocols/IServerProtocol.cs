using System;
using System.Security.Cryptography;

namespace RaspberryPi.Common.Protocols {
	public interface IServerProtocol : IProtocol {
		event EventHandler<MessageReceivedEventArgs> MessageReceived;

		void ParseMessage(byte[] message);
	}
}
