using GorudoYami.Common.Modules;
using RaspberryPi.Common.Models.Mqtt;

namespace RaspberryPi.Common.Modules.Providers;

public interface IMqttProvider : IModule {
	IReadOnlyDictionary<string, MqttTopic> Topics { get; }
	bool Connected { get; }

	Task ConnectAsync(CancellationToken cancellationToken = default);
	Task DisconnectAsync(CancellationToken cancellationToken = default);
	string? GetTopicValue(string topicName);
	Task PublishAsync(string topicName, string value, CancellationToken cancellationToken = default);
	Task SubscribeAsync(string topicName, CancellationToken cancellationToken = default);
	Task UnsubscribeAsync(string topicName, CancellationToken cancellationToken = default);
}
