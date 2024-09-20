namespace RaspberryPi.Common.Services {
	public interface IDrivingService : IInitializableService {
		void Backward(double power = 0.5);
		void Forward(double power = 0.5);
		void Left(double power = 1);
		void Right(double power = 1);
		void Stop();
		void Straight();
	}
}
