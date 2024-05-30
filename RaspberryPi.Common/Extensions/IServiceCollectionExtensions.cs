using Microsoft.Extensions.DependencyInjection;
using RaspberryPi.Common.Modules;

namespace RaspberryPi.Common.Extensions {
	public static class IServiceCollectionExtensions {
		public static IServiceCollection AddModule<TService, TImplementation>(this IServiceCollection serviceCollection)
			where TService : class, IModule
			where TImplementation : class, TService {
			serviceCollection.AddSingleton<TService, TImplementation>();
			return serviceCollection;
		}
	}
}
