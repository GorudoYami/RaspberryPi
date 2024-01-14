﻿using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RaspberryPi.Client.Models;
using RaspberryPi.Common.Protocols;

namespace RaspberryPi.Client.Tests {
	[TestFixture]
	public class ClientModuleTests {
		private ClientModule? _clientModule;

		private Mock<IOptions<ClientModuleOptions>> _mockedOptions;
		private Mock<IClientProtocol> _mockedClientProtocol;

		[SetUp]
		public void SetUp() {
			_mockedOptions = new Mock<IOptions<ClientModuleOptions>>();
			_mockedClientProtocol = new Mock<IClientProtocol>();
		}

		[TearDown]
		public void TearDown() {
			_clientModule?.Dispose();
			_clientModule = null;
		}

		private ClientModule GetInstance() {
			return _clientModule ??= new ClientModule(
				_mockedOptions.Object,
				_mockedClientProtocol.Object
			);
		}

		[Ignore("WIP")]
		[Test]
		public void Constructor() {
			_mockedOptions!.Setup(x => x.Value)
				.Returns(new ClientModuleOptions() {
					ServerHost = "localhost",
					ServerPort = 2137,
					TimeoutSeconds = 5000
				});

			Assert.DoesNotThrow(() => GetInstance());
		}


	}
}
