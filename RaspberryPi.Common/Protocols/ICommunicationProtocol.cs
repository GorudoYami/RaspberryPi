namespace RaspberryPi.Common.Protocols;

public interface ICommunicationProtocol {
	event EventHandler<MessageReceivedEventArgs>? MessageReceived;
	string Delimiter { get; }

	Task<Stream> InitializeCommunicationAsync(Stream stream, CancellationToken cancellationToken = default);
	void ParseMessage(byte[] message);
}
