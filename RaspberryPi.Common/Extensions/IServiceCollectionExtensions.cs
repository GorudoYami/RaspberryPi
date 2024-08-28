using Microsoft.Extensions.DependencyInjection;
using RaspberryPi.Common.Services;

namespace RaspberryPi.Common.Extensions;
public static class IServiceCollectionExtensions {
	public static IServiceCollection AddModule<TService, TImplementation>(this IServiceCollection serviceCollection)
		where TService : class, IService
		where TImplementation : class, TService {
		serviceCollection.AddSingleton<TService, TImplementation>();
		return serviceCollection;
	}
}
