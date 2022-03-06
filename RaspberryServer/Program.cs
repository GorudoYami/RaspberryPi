using System;

namespace RaspberryPi {
	public static class Program {
		public static void Main(string[] args) {
			using RaspberryServer server = new("localhost", 6969);

			Console.WriteLine("Starting server...");
			server.Start();
			Console.WriteLine("Done");

			Console.WriteLine("Starting client...");


			Console.ReadKey();

			Console.WriteLine("Stopping...");
			server.StopAsync();
			Console.WriteLine("Done");

			Console.ReadKey();
		}
	}
}