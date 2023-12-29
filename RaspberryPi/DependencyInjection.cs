using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaspberryPi.Camera;
using RaspberryPi.Client.Models;
using RaspberryPi.Client;
using RaspberryPi.Common.Modules;
using RaspberryPi.Driving.Models;
using RaspberryPi.Driving;
using RaspberryPi.Modem.Models;
using RaspberryPi.Modem;
using RaspberryPi.Sensors.Models;
using RaspberryPi.Sensors;
using GorudoYami.Common.Extensions;
using RaspberryPi.Common.Protocols;

namespace RaspberryPi;

public static class DependencyInjection {
	public static IServiceCollection AddProtocols(this IServiceCollection services) {
		return services
			.AddSingleton<IClientProtocol, EncryptedClientProtocol>()
			.AddSingleton<IServerProtocol, EncryptedServerProtocol>();
	}

	public static IServiceCollection AddModules(this IServiceCollection services) {
		return services
			.AddModule<IRaspberryPiModule, RaspberryPiModule>()
			.AddModule<ICameraModule, CameraModule>()
			.AddModule<ISensorsModule, SensorsModule>()
			.AddModule<IModemModule, ModemModule>()
			.AddModule<IDrivingModule, DrivingModule>()
			.AddModule<IClientModule, ClientModule>();
	}

	public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration) {
		services
			.AddOptions<ClientModuleOptions>()
			.Bind(configuration.GetRequiredSection(nameof(ClientModuleOptions)))
			.Validate(ClientModuleOptions.Validate)
			.ValidateOnStart();

		services
			.AddOptions<ModemModuleOptions>()
			.Bind(configuration.GetRequiredSection(nameof(ModemModuleOptions)))
			.Validate(ModemModuleOptions.Validate)
			.ValidateOnStart();

		services
			.AddOptions<DrivingModuleOptions>()
			.Bind(configuration.GetRequiredSection(nameof(DrivingModuleOptions)))
			.Validate(DrivingModuleOptions.Validate)
			.ValidateOnStart();

		services
			.AddOptions<SensorsModuleOptions>()
			.Bind(configuration.GetRequiredSection(nameof(SensorsModuleOptions)))
			.Validate(SensorsModuleOptions.Validate)
			.ValidateOnStart();

		return services;
	}
}
