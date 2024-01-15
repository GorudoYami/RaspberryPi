using GorudoYami.Common.Modules;

namespace RaspberryPi.Common.Modules {
	public interface ICameraModule : IModule {
		void Start();
		void Stop();
	}
}
