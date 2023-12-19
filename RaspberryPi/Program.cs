using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GorudoYami.Common.Extensions;
using RaspberryPi.Modules;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using RaspberryPi.Modules.Models;
using RaspberryPi.Common.Modules;

namespace RaspberryPi;

public static class Program {
	public static void Main() {
		using ServiceProvider serviceProvide = CreateServiceProvider();
		IRaspberryPiModule raspberryPi = serviceProvide.GetRequiredService<IRaspberryPiModule>();

		raspberryPi.Run();
	}

	public static ServiceProvider CreateServiceProvider() {
		IConfiguration configuration = new ConfigurationBuilder()
			.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
			.AddWritableAppSettings(reloadOnChange: true)
			.Build();

		IServiceCollection serviceCollection = new ServiceCollection()
			.AddSingleton(configuration)
			.AddModule<IRaspberryPiModule, RaspberryPiModule>()
			.AddModule<ITcpClientModule, TcpClientModule>()
			.AddLogging(builder => {
				builder.ClearProviders();
				builder.SetMinimumLevel(LogLevel.Debug);
				builder.AddNLog();
			});

		serviceCollection
			.AddOptions<TcpClientModuleOptions>()
			.Bind(configuration.GetRequiredSection(nameof(TcpClientModuleOptions)));

		return serviceCollection.BuildServiceProvider();
	}
}
