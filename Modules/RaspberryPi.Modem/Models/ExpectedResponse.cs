namespace RaspberryPi.Modem.Models;

public class ExpectedResponse {
	public required string Command { get; init; }
	public required IReadOnlyCollection<string> ResponseLines { get; init; }
	public required bool MatchAny { get; init; }
}
