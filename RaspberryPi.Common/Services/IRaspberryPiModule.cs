using System.Threading.Tasks;

namespace RaspberryPi.Common.Services;
public interface IRaspberryPiModule : IService {
	Task RunAsync();
}
