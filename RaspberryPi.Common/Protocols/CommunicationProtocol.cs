using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Protocols;
public class CommunicationProtocol : ICommunicationProtocol {
	public string Delimiter => "\r\n";
	public event EventHandler<MessageReceivedEventArgs> MessageReceived;

	private readonly List<byte> _messageTypes;

	public CommunicationProtocol() {
		_messageTypes = Enum.GetValues(typeof(MessageType)).Cast<byte>().ToList();
	}

	public Task<Stream> InitializeCommunicationAsync(Stream stream, CancellationToken cancellationToken = default) {
		return Task.FromResult(stream);
	}

	public void ParseMessage(byte[] message) {
		if (message.Length == 0) {
			throw new ProtocolException("Message to parse was empty");
		}
		else if (message.Length > 2) {
			throw new ProtocolException("Message was too big to parse");
		}

		MessageType messageType = MessageType.Unknown;
		byte messageValue = message.Length == 2 ? message[1] : (byte)0;
		if (_messageTypes.Any(x => x == message[0])) {
			messageType = (MessageType)message[0];
		}

		MessageReceived?.Invoke(this, new MessageReceivedEventArgs(messageType, messageValue));
	}

	private void ProcessMessage() {

	}
}
