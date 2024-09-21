using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Gpio;
using RaspberryPi.Common.Gpio.Pwm;
using RaspberryPi.Common.Services;
using RaspberryPi.Driving.Enums;
using RaspberryPi.Driving.Exceptions;
using RaspberryPi.Driving.Models;
using RaspberryPi.Driving.Options;
using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Driving {
	public class DrivingService
		: IDrivingService, IDisposable {
		public bool Enabled => _options.Enabled;
		public bool IsInitialized { get; private set; }

		private readonly ILogger<IDrivingService> _logger;
		private readonly DrivingOptions _options;
		private readonly IGpioControllerProvider _controller;
		private readonly Dictionary<DrivingPinType, IPwmChannelProvider> _pwmChannels = new Dictionary<DrivingPinType, IPwmChannelProvider>();
		private double _turnPower;
		private double _drivePower;
		private Direction? _turnDirection;
		private Direction? _driveDirection;

		public DrivingService(IOptions<DrivingOptions> options, ILogger<IDrivingService> logger, IGpioControllerProvider controller) {
			_logger = logger;
			_options = options.Value;
			_controller = controller;
		}

		public Task InitializeAsync(CancellationToken cancellationToken = default) {
			return Task.Run(async () => {
				foreach (DrivingPin pin in _options.Pins) {
					_controller.OpenPin(pin.Number, PinMode.Output);
				}

				foreach (DrivingPwmPin pwmPin in _options.PwmPins) {
					IPwmChannelProvider pwmChannel = _controller.GetPwmChannel(pwmPin.Chip, pwmPin.Channel, pwmPin.Frequency, 0);
					pwmChannel.Start();
					_pwmChannels.Add(pwmPin.Type, pwmChannel);
				}

				Left(1);
				await Task.Delay(2000);
				Right(1);
				await Task.Delay(2000);
				Straight();

				IsInitialized = true;
			}, cancellationToken);
		}

		private void Deinitialize() {
			foreach (DrivingPin pin in _options.Pins) {
				_controller.ClosePin(pin.Number);
			}

			foreach (IPwmChannelProvider pwmChannel in _pwmChannels.Values) {
				pwmChannel.Stop();
				(pwmChannel as IDisposable)?.Dispose();
			}
		}

		public void Left(double power = 1) {
			if (_turnDirection is Direction.Left && _turnPower == power) {
				return;
			}

			_logger.LogDebug("Turning left with {Power} power", power);
			_turnDirection = Direction.Left;
			_turnPower = power;
			UpdateTurnPins();
		}

		public void Right(double power = 1) {
			if (_turnDirection is Direction.Right && _turnPower == power) {
				return;
			}

			_logger.LogDebug("Turning right with {Power} power", power);
			_turnDirection = Direction.Right;
			_turnPower = power;
			UpdateTurnPins();
		}

		public void Straight() {
			_logger.LogDebug("Resetting turn");
			_turnDirection = null;
			_turnPower = 0;
			UpdateTurnPins();
		}

		public void Forward(double power = 0.5) {
			if (_driveDirection == Direction.Forward && _drivePower == power) {
				return;
			}

			_logger.LogDebug("Going forward with {Power} power", power);
			_driveDirection = Direction.Forward;
			_drivePower = power;
			UpdateDrivePins();
		}

		public void Backward(double power = 0.5) {
			if (_driveDirection is Direction.Back && _drivePower == power) {
				return;
			}

			_logger.LogDebug("Going back with {Power} power", power);
			_driveDirection = Direction.Back;
			_drivePower = power;
			UpdateDrivePins();
		}

		public void Stop() {
			_driveDirection = null;
			_turnDirection = null;
			_drivePower = 0;
			_turnPower = 0;

			_logger.LogDebug("Stopping");
			UpdateDrivePins();
			UpdateTurnPins();
		}

		private void UpdateTurnPins() {
			if (_turnDirection == null) {
				_pwmChannels[DrivingPinType.Steering].DutyCycle = 0.5;
			}
			else if (_turnDirection == Direction.Left) {
				_pwmChannels[DrivingPinType.Steering].DutyCycle = 1.0;
			}
			else if (_turnDirection == Direction.Right) {
				_pwmChannels[DrivingPinType.Steering].DutyCycle = 0.0;
			}

			_logger.LogTrace("{DutyCycle}", _pwmChannels[DrivingPinType.Steering].DutyCycle);
		}

		private void UpdateDrivePins() {
			if (_driveDirection == null) {
				Write(DrivingPinType.Forward, PinValue.Low);
				Write(DrivingPinType.Backward, PinValue.Low);
				_pwmChannels[DrivingPinType.Driving].DutyCycle = 0;
			}
			else if (_driveDirection == Direction.Forward) {
				Write(DrivingPinType.Backward, PinValue.Low);
				Write(DrivingPinType.Forward, PinValue.High);
				_pwmChannels[DrivingPinType.Driving].DutyCycle = _drivePower;
			}
			else if (_driveDirection == Direction.Back) {
				Write(DrivingPinType.Forward, PinValue.Low);
				Write(DrivingPinType.Backward, PinValue.High);
				_pwmChannels[DrivingPinType.Driving].DutyCycle = _drivePower;
			}
			else {
				throw new InvalidDirectionException($"Drive direction is invalid: {_driveDirection}.");
			}
		}

		private void Write(DrivingPinType type, PinValue pinValue) {
			DrivingPin pin = _options.Pins.Single(p => p.Type == type);
			_controller.Write(pin.Number, pinValue);
		}

		public void Dispose() {
			GC.SuppressFinalize(this);
			Stop();
			Deinitialize();
		}
	}
}
