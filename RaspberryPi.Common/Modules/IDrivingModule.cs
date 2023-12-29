using GorudoYami.Common.Modules;

namespace RaspberryPi.Common.Modules;

public interface IDrivingModule : IModule {
	void Backward(double power = 0.5);
	void Forward(double power = 0.5);
	void Left(double power = 1);
	void Right(double power = 1);
	void Stop();
	void Straight();
}
