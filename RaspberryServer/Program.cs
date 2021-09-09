using System;

namespace RaspberryServer {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Testing...");
            Server server = new("localhost", 6666);

            Console.WriteLine("Starting...");
            server.Start();
            Console.WriteLine("Done!");
            Console.ReadKey();
            Console.WriteLine("Stopping...");
            server.Stop();
            Console.WriteLine("Done!");
            Console.ReadKey();
        }
    }
}
