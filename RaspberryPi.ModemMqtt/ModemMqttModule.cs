using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Common.Models.Mqtt;
using RaspberryPi.Common.Modules;
using RaspberryPi.ModemMqtt.Models;

namespace RaspberryPi.ModemMqtt;

public class ModemMqttModule : IModemMqttModule {
	public bool LazyInitialization => false;
	public bool IsInitialized { get; private set; }
	public bool Connected => _modem.SendCommand("AT+SMSTATE?", expectedResponse: "+SMSTATE: 1");
	public IReadOnlyDictionary<string, MqttTopic> Topics => _topics;

	private readonly ModemMqttModuleOptions _options;
	private readonly ILogger<IModemMqttModule> _logger;
	private readonly IModemModule _modem;
	private readonly Dictionary<string, MqttTopic> _topics;

	public ModemMqttModule(IOptions<ModemMqttModuleOptions> options, ILogger<IModemMqttModule> logger, IModemModule modem) {
		_options = options.Value;
		_logger = logger;
		_modem = modem;
		_topics = [];
	}

	public Task InitializeAsync(CancellationToken cancellationToken = default) {
		return Task.Run(() => {
			_modem.SendCommand($"AT+SMCONF=\"URL\",\"{_options.ServerHost}\",\"{_options.ServerPort}\"");
			_modem.SendCommand($"AT+SMCONF=\"QOS\",{_options.QosLevel}");
			IsInitialized = true;
		}, cancellationToken);
	}

	public async Task ConnectAsync(CancellationToken cancellationToken = default) {
		if (Connected) {
			await DisconnectAsync(cancellationToken);
		}

		await Task.Run(() => {
			_modem.SendCommand("AT+SMCONN", throwOnFail: true);
			_modem.SendCommand("AT+SMSTATE?", expectedResponse: "+SMSTATE: 1", throwOnFail: true);
		}, cancellationToken);
	}

	public Task DisconnectAsync(CancellationToken cancellationToken = default) {
		return Task.Run(() => _modem.SendCommand("AT+SMDISC"), cancellationToken);
	}

	public string? GetTopicValue(string topicName) {
		return Topics.TryGetValue(topicName, out var value) ? value.Value : null;
	}

	public Task PublishAsync(string topicName, string value, CancellationToken cancellationToken = default) {
		return Task.Run(() => {
			AssertConnected();

			_modem.SendCommand("AT+SMPUB=\"{topicName}\",2, ");
		}, cancellationToken);
	}

	public Task SubscribeAsync(string topicName, CancellationToken cancellationToken = default) {
		return Task.Run(() => {
			AssertConnected();

			if (_topics.ContainsKey(topicName)) {
				return;
			}

			try {
				_topics.Add(topicName, new MqttTopic(topicName, null));
				_modem.SendCommand($"AT+SMSUB=\"{topicName}\",2", throwOnFail: true);
			}
			finally {
				_topics.Remove(topicName);
			}
		}, cancellationToken);
	}

	public Task UnsubscribeAsync(string topicName, CancellationToken cancellationToken = default) {
		return Task.Run(() => {
			AssertConnected();

			_modem.SendCommand($"AT+SMUNSUB=\"{topicName}\"", throwOnFail: true);
			_topics.Remove(topicName);
		}, cancellationToken);
	}

	private void AssertConnected() {
		if (Connected == false) {
			throw new InvalidOperationException("Not connected to a server");
		}
	}
}
