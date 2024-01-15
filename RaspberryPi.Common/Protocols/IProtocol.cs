using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Protocols {
	public interface IProtocol {
		Task<Stream> InitializeCommunicationAsync(Stream stream, CancellationToken cancellationToken = default);
	}
}
