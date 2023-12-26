using RaspberryPi.Common.Modules;

namespace RaspberryPi;

public class RaspberryPiModule : IRaspberryPiModule {
	public bool IsInitialized { get; private set; }
	private readonly IClientModule _tcpClientModule;

	public RaspberryPiModule(IClientModule tcpClientModule) {
		_tcpClientModule = tcpClientModule;
	}

	public async Task InitializeAsync(CancellationToken cancellationToken = default) {
		await Task.Delay(1);
	}

	public void Run() {
		while (true) {
			Thread.Sleep(100);
		}
	}
}
