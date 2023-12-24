using RaspberryPi.Common.Modules;

namespace RaspberryPi;

public class RaspberryPiModule : IRaspberryPiModule {
	private readonly IClientModule _tcpClientModule;

	public RaspberryPiModule(IClientModule tcpClientModule) {
		_tcpClientModule = tcpClientModule;
	}

	public void Run() {
		while (true) {
			Thread.Sleep(100);
		}
	}
}
