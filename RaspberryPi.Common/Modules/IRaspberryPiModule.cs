using GorudoYami.Common.Modules;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Modules {
	public interface IRaspberryPiModule : IModule {
		Task RunAsync();
	}
}
