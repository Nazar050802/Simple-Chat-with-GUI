using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerSide
{
    internal class CommunicationWithClient
    {
        private TcpListener Listener { get; set; }
        private ConcurrentDictionary<string, TcpClient> clients;
        
        public CommunicationWithClient(TcpListener listener)
        {
            Listener = listener;
            clients = new ConcurrentDictionary<string, TcpClient>();
        }

        public async Task HandleClientAsync(TcpClient client)
        {
            string clientKey = client.Client.RemoteEndPoint.ToString();
            clients.TryAdd(clientKey, client);

            Console.WriteLine("Client connected: " + clientKey);

            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[Constants.BufferSize];
                int bytesRead;

                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Received from " + clientKey + ": " + message);

                    // Broadcast the received message to all connected clients
                    BroadcastMessage(message, clientKey);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred with client " + clientKey + ": " + ex.Message);
            }
            finally
            {
                clients.TryRemove(clientKey, out _);
                client.Close();
                Console.WriteLine("Client disconnected: " + clientKey);
            }
        }

        public void BroadcastMessage(string message, string senderKey)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);

            foreach (var kvp in clients)
            {
                if (kvp.Key != senderKey)
                {
                    TcpClient client = kvp.Value;
                    NetworkStream stream = client.GetStream();
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        private void HandleTimeoutAsync(Exception ex, Socket Handler)
        {
            string timeOutMessage = "The connection was closed due to a timeout";

            Console.WriteLine(timeOutMessage);

            HandleException(ex, "[INFO]");
        }

        private void HandleException(Exception ex, string additionalText = "")
        {
            SimpleLogs.WriteToFile($"[CommunicationWithClient.cs] {additionalText} " + ex.ToString());
        }

        private bool CloseConnection(TcpClient client)
        {
            try
            {
                string clientKey = client.Client.RemoteEndPoint.ToString();
                clients.TryRemove(clientKey, out _);
                client.Close();

                Console.WriteLine("Client disconnected: " + clientKey);
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[CommunicationWithClient.cs][ERROR] " + ex.ToString());
                return false;
            }

            return true;
        }
    }

    class InteractWithClient
    {
        private TcpListener Handler { get; set; }

        public InteractWithClient(TcpListener handler)
        {
            Handler = handler;
        }
    }
}
