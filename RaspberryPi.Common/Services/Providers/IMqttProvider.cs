using RaspberryPi.Common.Models.Mqtt;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RaspberryPi.Common.Services.Providers;
public interface IMqttProvider : IService {
	IReadOnlyDictionary<string, MqttTopic> Topics { get; }

	string GetTopicValue(string topicName);
	Task PublishAsync(string topicName, string value, CancellationToken cancellationToken = default);
	Task SubscribeAsync(string topicName, CancellationToken cancellationToken = default);
	Task UnsubscribeAsync(string topicName, CancellationToken cancellationToken = default);
}
