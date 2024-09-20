using System.Threading;

namespace RaspberryPi.Common.Utilities {
	public interface ICancellationTokenProvider {
		void Cancel();
		CancellationToken GetToken();
	}
}
