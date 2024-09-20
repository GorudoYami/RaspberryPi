using System;
using System.Threading;

namespace RaspberryPi.Common.Utilities {
	public class CancellationTokenProvider : ICancellationTokenProvider, IDisposable {
		private readonly CancellationTokenSource _cancellationTokenSource;

		public CancellationTokenProvider() {
			_cancellationTokenSource = new CancellationTokenSource();
		}

		public CancellationToken GetToken() {
			if (_cancellationTokenSource.IsCancellationRequested) {
				throw new InvalidOperationException("Cancellation was requested");
			}

			return _cancellationTokenSource.Token;
		}

		public void Cancel() {
			_cancellationTokenSource.Cancel();
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			_cancellationTokenSource.Dispose();
		}
	}
}
