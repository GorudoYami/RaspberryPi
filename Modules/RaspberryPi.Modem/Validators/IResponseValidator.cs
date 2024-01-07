
namespace RaspberryPi.Modem.Validators;

public interface IResponseValidator {
	bool Validate(string command, IEnumerable<string> responseLines);
}
