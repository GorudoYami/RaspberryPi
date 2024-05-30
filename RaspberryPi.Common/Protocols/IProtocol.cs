using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Protocols {
	public interface IProtocol {
		string Delimiter { get; }

		Task<Stream> InitializeCommunicationAsync(Stream stream, CancellationToken cancellationToken = default);
	}
}
