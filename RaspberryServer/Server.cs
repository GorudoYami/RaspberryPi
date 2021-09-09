using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace RaspberryServer {
    public class Server {
        private const int rsaKeySize = 8000; // in bits
        private const int aesKeySize = 256;
        private readonly IPHostEntry host;
        private readonly IPAddress ipAddress;
        private readonly IPEndPoint localEndPoint;
        private readonly Dictionary<Socket, Aes> clients;
        // Change
        private string appKey;
        private Socket listener;
        private CancellationTokenSource tokenSource;
        private Task loopTask;

        public Server(string hostname, int port) {
            clients = new Dictionary<Socket, Aes>();

            // Get IP from hostname
            try {
                host = Dns.GetHostEntry(hostname);
                ipAddress = host.AddressList[0];
                localEndPoint = new IPEndPoint(ipAddress, port);
            }
            catch (Exception e) {
                Console.WriteLine(e.Source);
                Console.WriteLine(e.Message);
            }
        }

        public bool Start() {
            // Bind and listen on socket
            try {
                listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(5);
            }
            catch (Exception e) {
                Console.WriteLine(e.Source);
                Console.WriteLine(e.Message);
                return false;
            }

            // Start accept loop
            tokenSource = new CancellationTokenSource();
            loopTask = Task.Run(() => AcceptLoop(tokenSource.Token), tokenSource.Token);
            return true;
        }

        public async void Stop() {
            // Send cancel request and wait for the task
            tokenSource.Cancel();
            await loopTask;
            tokenSource.Dispose();

            // Cleanup
            foreach (Socket client in clients.Keys) {
                client.Disconnect(false);
                client.Close();
                client.Dispose();
            }
            clients.Clear();

            listener.Close();
            listener.Dispose();
        }

        private async void AcceptLoop(CancellationToken token) {
            // Accepting loop
            while (!token.IsCancellationRequested) {
                // This blocks v because it needs to return a valid client, fix it
                Socket client = await listener.AcceptAsync();
                if (client != null && client.Connected) {
                    client.Blocking = false;

                    using RSA rsa = RSA.Create(rsaKeySize);
                    // Send server public key
                    await SendUnencryptedAsync(client, rsa.ExportRSAPublicKey());

                    // Receive client public key
                    byte[] buffer = await ReceiveUnencryptedAsync(client, 15);
                    rsa.ImportRSAPublicKey(buffer, out _);

                    Aes aes = Aes.Create();
                    aes.KeySize = aesKeySize;
                    aes.GenerateKey();
                    aes.GenerateIV();

                    // Send encrypted AES key
                    await SendUnencryptedAsync(client, rsa.Encrypt(aes.Key, RSAEncryptionPadding.OaepSHA512));

                    // Receive secret ID value
                    buffer = await ReceiveAsync(client, 15);
                    if (Encoding.ASCII.GetString(buffer) != appKey) {
                        client.Disconnect(false);
                        client.Close();
                        client.Dispose();
                        continue;
                    }
                    clients[client] = aes;
                }
            }
        }

        private static async Task SendUnencryptedAsync(Socket client, byte[] data) {
            if (await client.SendAsync(data, SocketFlags.None) < data.Length)
                throw new Exception("Client disconnected during transfer of data.");
            if (await client.SendAsync(Encoding.ASCII.GetBytes("\r\n"), SocketFlags.None) < 2)
                throw new Exception("Client disconnected during transfer of data.");
        }

        private static async Task<byte[]> ReceiveUnencryptedAsync(Socket client, int timeout) {
            string data = string.Empty;
            byte[] buffer = new byte[1024];
            DateTime start = DateTime.Now;

            // Receive data (wait until "\r\n" or timeout)
            while (!data.Contains("\r\n") && start.Subtract(DateTime.Now).TotalSeconds < timeout) {
                if (client.Available > 0) {
                    if (await client.ReceiveAsync(buffer, SocketFlags.None) < 1)
                        throw new Exception("Client disconnected during transfer of data.");
                    data += Encoding.ASCII.GetString(buffer);
                    Array.Clear(buffer, 0, buffer.Length);
                }
            }

            if (start.Subtract(DateTime.Now).TotalSeconds >= timeout)
                throw new Exception("Client took too long to send a message. Timed out.");

            // Erase "\r\n" from data
            data = data[0..^2];
            return Encoding.ASCII.GetBytes(data);
        }

        public async Task SendAsync(Socket client, byte[] data) {
            // Calculate loop count and create output buffer
            int blockSize = clients[client].BlockSize;
            int loops = data.Length / blockSize;
            byte[] output = new byte[data.Length];

            // Encrypt blocks
            using var encryptor = clients[client].CreateEncryptor();
            for (int i = 0; i < loops; i++)
                encryptor.TransformBlock(data, i * blockSize, blockSize, output, i * blockSize);

            // Encrypt last block
            if (data.Length % blockSize != 0)
                encryptor.TransformFinalBlock(data, loops * blockSize, data.Length % blockSize);

            // Send data
            await client.SendAsync(output, SocketFlags.None);
            await client.SendAsync(Encoding.ASCII.GetBytes("\r\n"), SocketFlags.None);
        }

        public async Task<byte[]> ReceiveAsync(Socket client, int timeout) {
            string data = string.Empty;
            byte[] buffer = new byte[1024];
            DateTime start = DateTime.Now;

            // Receive data (wait until "\r\n" or timeout)
            while (!data.Contains("\r\n") && start.Subtract(DateTime.Now).TotalSeconds < timeout) {
                if (client.Available > 0) {
                    if (await client.ReceiveAsync(buffer, SocketFlags.None) < 1)
                        throw new Exception("Client disconnected during transfer of data.");
                    data += Encoding.ASCII.GetString(buffer);
                    Array.Clear(buffer, 0, buffer.Length);
                }
            }

            if (start.Subtract(DateTime.Now).TotalSeconds >= timeout)
                throw new Exception("Client took too long to send a message. Timed out.");

            // Remove last 2 characters ("\r\n")
            data = data[0..^2];

            using var decryptor = clients[client].CreateDecryptor();

            // Copy data to buffer
            buffer = Encoding.ASCII.GetBytes(data);
            // Create output buffer
            byte[] output = new byte[buffer.Length];
            int blockSize = clients[client].BlockSize;
            // Calculate loop count
            int loops = buffer.Length / blockSize;

            // Encrypt data
            for (int i = 0; i < loops; i++)
                decryptor.TransformBlock(buffer, i * blockSize, blockSize, output, i * blockSize);
            data = Encoding.ASCII.GetString(output);

            // Encrypt last block
            if (buffer.Length % blockSize != 0)
                data += decryptor.TransformFinalBlock(buffer, loops * blockSize, buffer.Length % blockSize);

            return Encoding.ASCII.GetBytes(data);
        }
    }
}
