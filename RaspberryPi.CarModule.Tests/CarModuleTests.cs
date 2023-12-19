using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.CarModule.Exceptions;
using RaspberryPi.CarModule.Models;

namespace RaspberryPi.CarModule.Tests;

public class CarModuleTests {
	private CarModule? _carModule;
	private Mock<IOptions<CarModuleOptions>>? _mockedOptions;

	[SetUp]
	public void SetUp() {
		_mockedOptions = new Mock<IOptions<CarModuleOptions>>();
	}

	private CarModule GetInstance() {
		return _carModule ??= new CarModule(_mockedOptions!.Object);
	}

	[Ignore("WIP")]
	[Test]
	public void Constructor_IncompletePins_ThrowException() {
		_mockedOptions!.Setup(x => x.Value)
			.Returns(new CarModuleOptions(new List<IDriverPin>()));

		Assert.Throws<IncompletePinMappingException>(() => GetInstance());
	}

	[TearDown]
	public void TearDown() {
		_carModule?.Dispose();
	}
}
