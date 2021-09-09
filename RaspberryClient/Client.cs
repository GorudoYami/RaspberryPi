using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RaspberryClient {
    public class Client {
        private Socket server;
        private const int rsaKeySize = 8000;
        private const int aesKeySize = 256;
        private readonly IPAddress ipAddress;

        public Client(string hostname, int port) {
            try {
                IPHostEntry host = Dns.GetHostEntry(hostname);
                ipAddress = host.AddressList[0];
            }
            catch (Exception e) {
                Console.WriteLine(e.Source);
                Console.WriteLine(e.Message);
            }
        }

        public bool Start() {
            try {
                server = Socket.ConnectAsync(SocketType.Stream, ProtocolType.Tcp)
            }
        }
    }
}
