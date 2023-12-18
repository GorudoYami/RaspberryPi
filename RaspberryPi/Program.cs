using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GorudoYami.Common.Extensions;
using RaspberryPi.Modules;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using RaspberryPi.Modules.Models;

namespace RaspberryPi;

public static class Program {
	public static void Main() {
		using ServiceProvider serviceProvide = CreateServiceProvider();
		IRaspberryPi raspberryPi = serviceProvide.GetRequiredService<IRaspberryPi>();

		raspberryPi.Run();
	}

	public static ServiceProvider CreateServiceProvider() {
		IConfiguration configuration = new ConfigurationBuilder()
			.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
			.AddWritableAppSettings(reloadOnChange: true)
			.Build();

		IServiceCollection serviceCollection = new ServiceCollection()
			.AddSingleton(configuration)
			.AddModule<IRaspberryPi, RaspberyPi>()
			.AddModule<ITcpClientModule, TcpClientModule>()
			.AddLogging(builder => {
				builder.ClearProviders();
				builder.SetMinimumLevel(LogLevel.Debug);
				builder.AddNLog();
			});

		serviceCollection
			.AddOptions<TcpClientOptions>()
			.Bind(configuration.GetRequiredSection(nameof(TcpClientOptions)));

		return serviceCollection.BuildServiceProvider();
	}
}
