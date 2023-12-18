using GorudoYami.Common.Modules;
using RaspberryPi.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaspberryPi;

public interface IRaspberryPi : IModule {
	void Run();
}

public class RaspberyPi : IRaspberryPi {
	private readonly ITcpClientModule _tcpClientModule;

	public RaspberyPi(ITcpClientModule tcpClientModule) {
		_tcpClientModule = tcpClientModule;
	}

	public void Run() {
		while (true) {
			Thread.Sleep(100);
		}
	}
}
