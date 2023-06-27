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

        private InteractWithServer interactWithServer;

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
                    await Task.Delay(Constants.DelayForReconnect);
                }
            }

            interactWithServer = new InteractWithServer(Client, new RSAGenerating(), new RSAGenerating());
        }

        public async Task InitialSetting(string username)
        {
            // Get server public key
            interactWithServer.rsaGeneratingServer.SetPublicKeyFromString(await interactWithServer.ReceiveMessageAsync());

            // Send client public key to server
            await interactWithServer.SendMessageAsync(interactWithServer.rsaGeneratingClient.PublicKey);

            // Send user username
            await interactWithServer.SendMessageWithEncryptionAsync(username);
        }

        public void StartToReveiveMessages()
        {
            _ = Task.Run(() => ReceiveMessagesAsync(cancellationTokenSource.Token));
        }

        public async Task StartToSendMessages()
        {
            Console.WriteLine("Enter messages to send (press Enter to send, type 'exit' to disconnect):");

            string message;
            do
            {
                message = Console.ReadLine();
                await interactWithServer.SendMessageWithEncryptionAsync(message);
            } while (message != "exit");
        }

        public async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
        {
            try
            {
                string message = await interactWithServer.ReceiveMessageWithEncryptionAsync();
                while (!cancellationToken.IsCancellationRequested && message != "")
                {
                    Console.WriteLine(message);
                    message = await interactWithServer.ReceiveMessageWithEncryptionAsync();
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

        public bool CloseConnection()
        {
            try
            {
                interactWithServer.CloseStreamConnection();
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
        private NetworkStream Stream { get; set; }

        public RSAGenerating rsaGeneratingServer;

        public RSAGenerating rsaGeneratingClient;

        public InteractWithServer(TcpClient client, RSAGenerating rsaClient, RSAGenerating rsaServer)
        {
            Stream = client.GetStream();

            rsaGeneratingClient = rsaClient;
            rsaGeneratingServer = rsaServer;
        }

        public async Task<string> ReceiveMessageWithEncryptionAsync()
        {
            string message = "";
            try
            {
                byte[] buffer = new byte[Constants.BufferSize];
                int bytesRead;

                // Read the message
                bytesRead = await Stream.ReadAsync(buffer, 0, buffer.Length);
                message = rsaGeneratingClient.DecryptIntoString(buffer);

            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[CommunicationWithClient.cs][ERROR] " + ex.ToString());
            }

            return message;
        }

        public async Task<string> ReceiveMessageAsync()
        {
            string message = "";
            try
            {
                byte[] buffer = new byte[Constants.BufferSize];
                int bytesRead;

                // Read the message
                bytesRead = await Stream.ReadAsync(buffer, 0, buffer.Length);
                message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[CommunicationWithClient.cs][ERROR] " + ex.ToString());
            }

            return message;
        }

        public async Task SendMessageWithEncryptionAsync(string message)
        {
            try
            {
                // Send message
                byte[] buffer = rsaGeneratingServer.EncryptString(message);
                await Stream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[CommunicationWithClient.cs][ERROR] " + ex.ToString());
            }
        }

        public async Task SendMessageAsync(string message)
        {
            try
            {
                // Send message
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await Stream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[CommunicationWithClient.cs][ERROR] " + ex.ToString());
            }
        }

        public void CloseStreamConnection()
        {
            Stream.Close();
        }
    }
}