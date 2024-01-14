using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Events;
using RaspberryPi.Common.Gpio;
using RaspberryPi.Common.Modules;
using RaspberryPi.Sensors.Enums;
using RaspberryPi.Sensors.Models;
using System.Device.Gpio;
using System.Diagnostics;

namespace RaspberryPi.Sensors {
	public class SensorsModule(IOptions<SensorsModuleOptions> options, ILogger<ISensorsModule> logger, IGpioControllerProvider controller)
		: ISensorsModule, IDisposable, IAsyncDisposable {
		public bool LazyInitialization => false;
		public bool IsInitialized { get; private set; }

		public event EventHandler<SensorTriggeredEventArgs>? SensorTriggered;

		private readonly ICollection<Sensor> _sensors = options.Value.Sensors;
		private readonly ILogger<ISensorsModule> _logger = logger;
		private readonly IGpioControllerProvider _controller = controller;
		private readonly int _reportDistance = options.Value.ReportDistance;
		private readonly int _poolingPeriodSeconds = options.Value.PoolingPeriod;
		private CancellationTokenSource? _cancellationTokenSource;
		private Task? _mainTask;

		public Task InitializeAsync(CancellationToken cancellationToken = default) {
			return Task.Run(() => {
				foreach (Sensor sensor in _sensors) {
					foreach (int pinNumber in sensor.Pins
						.Where(x => x.Key == SensorPinType.Echo)
						.Select(x => x.Value)) {
						_controller.OpenPin(pinNumber, PinMode.Input);
					}

					foreach (int pinNumber in sensor.Pins
						.Where(x => x.Key == SensorPinType.Trig)
						.Select(x => x.Value)) {
						_controller.OpenPin(pinNumber, PinMode.Output);
					}
				}
			}, cancellationToken);
		}

		public void Start() {
			if (_mainTask != null && _cancellationTokenSource != null) {
				_cancellationTokenSource = new CancellationTokenSource();
				_mainTask = Run(_cancellationTokenSource.Token);
			}
		}

		private async Task Run(CancellationToken cancellationToken) {
			while (cancellationToken.IsCancellationRequested == false) {
				foreach (Sensor sensor in _sensors) {
					int distance = Measure(sensor);

					if (sensor.IsTriggered() == false && _reportDistance >= distance) {
						_logger.LogDebug("[{SensorName}] Triggered at {Distance}", sensor.Name, distance);
						sensor.SetTriggered();
						SensorTriggered?.Invoke(this, new SensorTriggeredEventArgs(sensor.Name, distance));
					}
					else if (sensor.IsTriggered() && _reportDistance <= distance) {
						_logger.LogDebug("[{SensorName}] Resetting sensor at {Distance}", sensor.Name, distance);
						sensor.Reset();
					}
				}

				if (_poolingPeriodSeconds != 0) {
					await Task.Delay(_poolingPeriodSeconds * 1000, cancellationToken);
				}
			}
		}

		private int Measure(Sensor sensor) {
			int trigPinNumber = sensor.Pins[SensorPinType.Trig];
			_controller.Write(trigPinNumber, PinValue.High);
			_controller.Write(trigPinNumber, PinValue.Low);

			int echoPinNumber = sensor.Pins[SensorPinType.Echo];
			WaitUntil(() => _controller.Read(echoPinNumber), PinValue.High);
			var stopwatch = Stopwatch.StartNew();
			WaitUntil(() => _controller.Read(echoPinNumber), PinValue.Low);
			stopwatch.Stop();

			return (int)Math.Round(stopwatch.Elapsed.TotalMicroseconds / 58, 0);
		}

		private static void WaitUntil(Func<PinValue> queryAction, PinValue targetPinValue) {
			while (queryAction() != targetPinValue) ;
		}

		public void ResetSensor(string sensorName) {
			_sensors.Single(x => x.Name == sensorName).Reset();
		}

		public async Task StopAsync() {
			if (_mainTask != null && _cancellationTokenSource != null) {
				_cancellationTokenSource.Cancel();
				await _mainTask;
				_cancellationTokenSource.Dispose();
				_cancellationTokenSource = null;
				_mainTask.Dispose();
				_mainTask = null;
			}
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			StopAsync().GetAwaiter().GetResult();
		}

		public async ValueTask DisposeAsync() {
			GC.SuppressFinalize(this);
			await StopAsync();
		}
	}
}
