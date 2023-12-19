using RaspberryPi.Common.Modules;


namespace RaspberryPi;

public class RaspberryPiModule : IRaspberryPiModule {
	private readonly ITcpClientModule _tcpClientModule;

	public RaspberryPiModule(ITcpClientModule tcpClientModule) {
		_tcpClientModule = tcpClientModule;
	}

	public void Run() {
		while (true) {
			Thread.Sleep(100);
		}
	}
}
