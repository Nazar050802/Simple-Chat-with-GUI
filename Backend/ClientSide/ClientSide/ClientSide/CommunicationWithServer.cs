using ServerSide;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientSide
{
    internal class CommunicationWithServer
    {
        private TcpClient Client { get; set; }
        private CancellationTokenSource cancellationTokenSource;
        private BasicClient BasicClientInfo { get; set; }

        public CommunicationWithServer(TcpClient client, BasicClient basicClientInfo)
        {
            Client = client;
            BasicClientInfo = basicClientInfo;
            cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task ConnectClientAsync()
        {
            await Client.ConnectAsync(BasicClientInfo.IpAddress, BasicClientInfo.Port);
        }

        public async Task EstablishConnectionWithServer()
        {
            while (!IsClientConnected())
            {
                try
                {
                    await ConnectClientAsync();
                }
                catch (Exception ex)
                {
                    SimpleLogs.WriteToFile("[CommunicationWithServer.cs][ERROR] " + ex.ToString());
                    await Task.Delay(1000);
                }
            }
        }

        public void StartToReveiveMessages()
        {
            InteractWithServer interactWithServer = new InteractWithServer();
            _ = Task.Run(() => interactWithServer.ReceiveMessages(Client, cancellationTokenSource.Token));
        }

        public async Task StartToSendMessages()
        {
            InteractWithServer interactWithServer = new InteractWithServer();
            Console.WriteLine("Enter messages to send (press Enter to send, type 'exit' to disconnect):");

            string message;
            do
            {
                message = Console.ReadLine();
                await interactWithServer.SendMessage(Client, message);
            } while (message != "exit");
        }

        public bool CloseConnection()
        {
            try
            {
                cancellationTokenSource.Cancel();
                Client.Close();

                Console.WriteLine("The communication has finished.");
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[CommunicationWithServer.cs][ERROR] " + ex.ToString());
                return false;
            }

            return true;
        }

        private bool IsClientConnected()
        {
            return Client.Connected;
        }

    }

    class InteractWithServer
    {

        public async Task ReceiveMessages(TcpClient Client, CancellationToken cancellationToken)
        {
            try
            {
                NetworkStream stream = Client.GetStream();
                byte[] buffer = new byte[Constants.BufferSize];
                int bytesRead;

                while (!cancellationToken.IsCancellationRequested &&
                       (bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine(message);
                }
                
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[CommunicationWithServer.cs][ERROR] " + ex.ToString());
            }
            finally
            {
                Client.Close();
            }
        }

        public async Task SendMessage(TcpClient Client, string message)
        {
            try
            {
                NetworkStream stream = Client.GetStream();
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[CommunicationWithServer.cs][ERROR] " + ex.ToString());
            }
        }
    }
}