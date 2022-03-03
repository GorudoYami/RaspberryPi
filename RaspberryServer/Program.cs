using System;

namespace RaspberryPi;

class Program {
	static void Main(string[] args) {
		Console.WriteLine("Testing...");
		RaspberryServer server = new("localhost", 6666);

		Console.WriteLine("Starting...");
		server.Start();
		Console.WriteLine("Done!");
		Console.ReadKey();
		Console.WriteLine("Stopping...");
		server.StopAsync();
		Console.WriteLine("Done!");
		Console.ReadKey();
	}
}
