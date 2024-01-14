using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaspberryPi.Modem.Models;
using RaspberryPi.Modem.Options;

namespace RaspberryPi.Modem.Validators {
	public class ResponseValidator : IResponseValidator {
		private readonly ILogger<IResponseValidator> _logger;
		private readonly IEnumerable<IGrouping<string, ExpectedResponse>> _expectedResponseGroups;

		public ResponseValidator(IOptions<ModemModuleOptions> options, ILogger<IResponseValidator> logger) {
			_expectedResponseGroups = options.Value.ExpectedResponses.GroupBy(x => x.Command);
			_logger = logger;
		}

		public bool Validate(string command, IEnumerable<string> responseLines) {
			if (responseLines.Count() == 0) {
				_logger.LogError("No response lines for command {Command}", command);
				return false;
			}

			IEnumerable<ExpectedResponse>? expectedResponseGroup = _expectedResponseGroups.FirstOrDefault(x => x.Key == command);
			if (expectedResponseGroup == null) {
				_logger.LogInformation("No expected responses for command {Command} using default 'OK'", command);
				return responseLines.Any(x => x.Contains("OK"));
			}

			foreach (ExpectedResponse expectedResponse in expectedResponseGroup) {
				if (expectedResponse.MatchAny) {
					bool matchesAny = true;

					foreach (string response in responseLines) {
						matchesAny &= expectedResponse.ResponseLines.Any(response.Contains);
					}

					if (matchesAny) {
						return true;
					}
				}
				else if (expectedResponse.ResponseLines.Count == responseLines.Count()) {
					bool matches = responseLines
						.Join(expectedResponse.ResponseLines, x => x, y => y, (x, y) => x)
						.Count() == expectedResponse.ResponseLines.Count;

					if (matches) {
						return true;
					}
				}
			}

			return false;
		}
	}
}
