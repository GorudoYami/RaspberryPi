namespace RaspberryPi.Common.Protocols;

public interface IProtocol {
	Task<Stream> InitializeCommunicationAsync(Stream stream, CancellationToken cancellationToken = default);
}
