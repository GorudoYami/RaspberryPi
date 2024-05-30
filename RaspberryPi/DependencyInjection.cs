using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RaspberryPi.Camera;
using RaspberryPi.Camera.Options;
using RaspberryPi.Common.Modules;
using RaspberryPi.Common.Protocols;
using RaspberryPi.Common.Resolvers;
using RaspberryPi.Driving;
using RaspberryPi.Driving.Options;
using RaspberryPi.Options;
using RaspberryPi.Resolvers;
using RaspberryPi.Sensors;
using RaspberryPi.Sensors.Options;
using RaspberryPi.Common.Extensions;
using RaspberryPi.Server;

namespace RaspberryPi {
	public static class DependencyInjection {
		public static IServiceCollection AddResolvers(this IServiceCollection services) {
			return services
				.AddSingleton<INetworkingResolver, NetworkingResolver>();
		}

		public static IServiceCollection AddProtocols(this IServiceCollection services) {
			return services
				.AddSingleton<IClientProtocol, StandardClientProtocol>()
				.AddSingleton<IServerProtocol, StandardServerProtocol>();
		}

		public static IServiceCollection AddModules(this IServiceCollection services) {
			return services
				.AddModule<IRaspberryPiModule, RaspberryPiModule>()
				.AddModule<ICameraModule, CameraModule>()
				.AddModule<ISensorsModule, SensorsModule>()
				//.AddModule<IModemModule, ModemModule>()
				//.AddModule<IModemServerModule, ModemServerModule>()
				.AddModule<IDrivingModule, DrivingModule>()
				.AddModule<IServerModule, ServerModule>();
		}

		public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration) {
			services
				.AddOptions<RaspberryPiModuleOptions>()
				.Bind(configuration.GetRequiredSection(nameof(RaspberryPiModuleOptions)))
				.Validate(RaspberryPiModuleOptions.Validate)
				.ValidateOnStart();

			services
				.AddOptions<CameraModuleOptions>()
				.Bind(configuration.GetRequiredSection(nameof(CameraModuleOptions)))
				.Validate(CameraModuleOptions.Validate)
				.ValidateOnStart();

			services
				.AddOptions<SensorsModuleOptions>()
				.Bind(configuration.GetRequiredSection(nameof(SensorsModuleOptions)))
				.Validate(SensorsModuleOptions.Validate)
				.ValidateOnStart();

			//services
			//	.AddOptions<ModemModuleOptions>()
			//	.Bind(configuration.GetRequiredSection(nameof(ModemModuleOptions)))
			//	.Validate(ModemModuleOptions.Validate)
			//	.ValidateOnStart();

			services
				.AddOptions<DrivingModuleOptions>()
				.Bind(configuration.GetRequiredSection(nameof(DrivingModuleOptions)))
				.Validate(DrivingModuleOptions.Validate)
				.ValidateOnStart();

			return services;
		}
	}
}
