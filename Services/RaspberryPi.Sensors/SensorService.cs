using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Events;
using RaspberryPi.Common.Gpio;
using RaspberryPi.Common.Services;
using RaspberryPi.Sensors.Enums;
using RaspberryPi.Sensors.Models;
using RaspberryPi.Sensors.Options;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Sensors {
	public class SensorService
		: ISensorService, IDisposable {
		public bool Enabled { get; }
		public bool IsInitialized { get; private set; }

		public event EventHandler<SensorTriggeredEventArgs> SensorTriggered;

		private readonly IGpioControllerProvider _controller;
		private readonly ILogger<ISensorService> _logger;
		private readonly ICollection<Sensor> _sensors;
		private readonly int _reportDistance;
		private CancellationTokenSource _cancellationTokenSource;
		private Task _mainTask;

		public SensorService(IOptions<SensorOptions> options, ILogger<ISensorService> logger, IGpioControllerProvider controller) {
			Enabled = options.Value.Enabled;
			_logger = logger;
			_controller = controller;
			_sensors = options.Value.Sensors;
			_reportDistance = options.Value.ReportDistance;
		}

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
				IsInitialized = true;
			}, cancellationToken);
		}

		public void Start() {
			if (_mainTask == null && _cancellationTokenSource == null) {
				_cancellationTokenSource = new CancellationTokenSource();
				_mainTask = Task.Run(() => Run(_cancellationTokenSource.Token));
			}
		}

		private async Task Run(CancellationToken cancellationToken) {
			while (cancellationToken.IsCancellationRequested == false) {
				await Task.WhenAll(_sensors.Select(sensor => Task.Run(() => {
					int distance = Measure(sensor, cancellationToken);

					if (sensor.IsTriggered() == false && _reportDistance >= distance) {
						_logger.LogDebug("[{SensorName}] Triggered at {Distance}", sensor.Name, distance);
						sensor.SetTriggered();
						SensorTriggered?.Invoke(this, new SensorTriggeredEventArgs(sensor.Name, distance));
					}
					else if (sensor.IsTriggered() && _reportDistance <= distance) {
						_logger.LogDebug("[{SensorName}] Resetting sensor at {Distance}", sensor.Name, distance);
						sensor.Reset();
					}
				}, cancellationToken)));
			}
		}

		private int Measure(Sensor sensor, CancellationToken cancellationToken) {
			using (var cts = new CancellationTokenSource(500)) {
				using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, cts.Token)) {
					int trigPinNumber = sensor.Pins[SensorPinType.Trig];
					_controller.Write(trigPinNumber, PinValue.High);
					_controller.Write(trigPinNumber, PinValue.Low);
					_logger.LogDebug("Sending trig to sensor {SensorName}", sensor.Name);

					int echoPinNumber = sensor.Pins[SensorPinType.Echo];
					WaitUntil(() => _controller.Read(echoPinNumber), PinValue.High, linkedCts.Token);
					_logger.LogDebug("Echo high for sensor {SensorName}", sensor.Name);
					var stopwatch = Stopwatch.StartNew();
					WaitUntil(() => _controller.Read(echoPinNumber), PinValue.Low, linkedCts.Token);
					_logger.LogDebug("Echo low for sensor {SensorName}", sensor.Name);
					stopwatch.Stop();

					int result = (int)Math.Round(stopwatch.Elapsed.TotalMilliseconds * 1000 / 58, 0);
					_logger.LogDebug("Calculated result distance: {Distance}", result);
					return result;
				}
			}
		}

		private static void WaitUntil(Func<PinValue> queryAction, PinValue targetPinValue, CancellationToken cancellationToken) {
			while (queryAction() != targetPinValue && cancellationToken.IsCancellationRequested == false) {
			}
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
	}
}
