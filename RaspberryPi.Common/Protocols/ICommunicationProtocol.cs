using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Protocols {
	public interface ICommunicationProtocol {
		event EventHandler<MessageReceivedEventArgs> MessageReceived;
		string Delimiter { get; }

		Task<Stream> InitializeCommunicationAsync(Stream stream, CancellationToken cancellationToken = default);
		void ParseMessage(byte[] message);
	}
}
