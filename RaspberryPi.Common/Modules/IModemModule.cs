using GorudoYami.Common.Modules;
using System.Net;

namespace RaspberryPi.Common.Modules;

public interface IModemModule : INetworkModule {
	Task StartAsync(CancellationToken cancellationToken = default);
}
