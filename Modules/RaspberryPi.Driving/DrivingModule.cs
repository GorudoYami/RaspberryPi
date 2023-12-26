﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Gpio;
using RaspberryPi.Common.Gpio.Pwm;
using RaspberryPi.Common.Modules;
using RaspberryPi.Driving.Enums;
using RaspberryPi.Driving.Exceptions;
using RaspberryPi.Driving.Models;
using System.Device.Gpio;
using System.Device.Pwm;

namespace RaspberryPi.Driving;

public class DrivingModule : IDrivingModule, IDisposable {
	public bool IsInitialized { get; private set; }

	private readonly IGpioControllerProvider _controller;
	private readonly ILogger<IDrivingModule> _logger;
	private readonly ICollection<DrivingPin> _pins;
	private readonly Dictionary<Direction, IPwmChannelProvider> _pwmChannels;
	private double _turnPower;
	private double _drivePower;
	private Direction? _turnDirection;
	private Direction? _driveDirection;

	public DrivingModule(IOptions<DrivingModuleOptions> options, ILogger<IDrivingModule> logger, IGpioControllerProvider controller) {
		_pins = options.Value.Pins;
		_turnPower = 0;
		_drivePower = 0;
		_turnDirection = null;
		_driveDirection = null;
		_controller = controller;
		_logger = logger;
		_pwmChannels = [];
	}

	public Task InitializeAsync(CancellationToken cancellationToken = default) {
		return Task.Run(() => {
			foreach (DrivingPin pin in _pins) {
				if (pin is DrivingPwmPin pwmPin) {
					IPwmChannelProvider pwmChannel = _controller.GetPwmChannel(pwmPin.Chip, pwmPin.Number, 400, 0);
					_pwmChannels.Add(pin.Direction, pwmChannel);
				}
				else {
					_controller.OpenPin(pin.Number, PinMode.Output);
				}
			}
		}, cancellationToken);
	}

	private void Deinitialize() {
		foreach (DrivingPin pin in _pins.Where(x => x is not DrivingPwmPin)) {
			_controller.ClosePin(pin.Number);
		}

		foreach (IPwmChannelProvider pwmChannel in _pwmChannels.Values) {
			pwmChannel.Stop();
			pwmChannel.Dispose();
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

	public void Back(double power = 0.5) {
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
		// TODO: Change since it will be a servo
	}

	private void UpdateDrivePins() {
		if (_driveDirection == null) {
			StopPwm(Direction.Forward | Direction.Back);
			Write(Direction.Forward, PinValue.Low);
			Write(Direction.Back, PinValue.Low);
		}
		else if (_driveDirection == Direction.Forward) {
			Write(Direction.Back, PinValue.Low);
			Write(Direction.Forward, PinValue.High);
			StartPwm(Direction.Forward | Direction.Back);
		}
		else if (_driveDirection == Direction.Back) {
			Write(Direction.Forward, PinValue.Low);
			Write(Direction.Back, PinValue.High);
			StartPwm(Direction.Forward | Direction.Back);
		}
		else {
			throw new InvalidDirectionException($"Drive direction is invalid: {_driveDirection}.");
		}
	}

	private void Write(Direction direction, PinValue pinValue) {
		DrivingPin pin = _pins.Single(p => p.Direction == direction);
		_controller.Write(pin.Number, pinValue);
	}

	private void StartPwm(Direction direction) {
		_pwmChannels[direction].DutyCycle = _turnPower;
		_pwmChannels[direction].Start();
	}

	private void StopPwm(Direction direction) {
		_pwmChannels[direction].Stop();
	}

	public void Dispose() {
		GC.SuppressFinalize(this);
		Stop();
		Deinitialize();
	}
}
