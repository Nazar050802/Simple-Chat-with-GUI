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
        private ConcurrentBag<User> clients;
        
        public CommunicationWithClient(TcpListener listener)
        {
            clients = new ConcurrentBag<User>();
        }

        public async Task HandleClientAsync(TcpClient client)
        {
            User tempUserInfo = new User(client);

            InteractWithClient interactWithClient = new InteractWithClient(client, new RSAGenerating(), tempUserInfo.rsaGeneratingServer);

            Tuple<User, InteractWithClient> outputInitialSetting  = await InitialSetting(tempUserInfo, interactWithClient);
            tempUserInfo = outputInitialSetting.Item1;
            interactWithClient = outputInitialSetting.Item2;

            clients.Add(tempUserInfo);

            try
            {
                await StartReceiveAndSendMessagesAsync(tempUserInfo, interactWithClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred with client " + tempUserInfo.Id + ": " + ex.Message);
                HandleException(ex);
            }
            finally
            {
                interactWithClient.CloseStreamConnection();
                CloseConnection(client, tempUserInfo);
            }
        }

        public async Task StartReceiveAndSendMessagesAsync(User user, InteractWithClient interactWithClient)
        {
            string message = await interactWithClient.ReceiveMessageWithEncryptionAsync();
            while (message != "")
            {
                Console.WriteLine($"Received from {user.Id}({user.Name}): {message}");

                // Broadcast the received message to all connected clients
                BroadcastMessage(message, user);

                message = await interactWithClient.ReceiveMessageWithEncryptionAsync();
            }
        }

        public async Task<Tuple<User, InteractWithClient>> InitialSetting(User user, InteractWithClient interactWithClient)
        {
            try
            {
                // Send server public key to client
                await interactWithClient.SendMessageAsync(user.rsaGeneratingServer.PublicKey);

                // Get client public key
                user.rsaGeneratingClient.SetPublicKeyFromString(await interactWithClient.ReceiveMessageAsync());
                interactWithClient.rsaGeneratingClient = user.rsaGeneratingClient;

                // Get user username
                user.Name = await interactWithClient.ReceiveMessageWithEncryptionAsync();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return new Tuple<User, InteractWithClient>(user, interactWithClient);
        }

        public void BroadcastMessage(string message, User currentUser)
        {
            foreach (User user in clients)
            {
                if (user.Id != currentUser.Id)
                {
                    InteractWithClient interactWithClient = new InteractWithClient(user.TcpConnection, user.rsaGeneratingClient, user.rsaGeneratingServer);
                    _ = interactWithClient.SendMessageWithEncryptionAsync(message);
                }
            }
        }

        private void RemoveItemFromConcurrentBag(User userToRemove)
        {
            ConcurrentBag<User> newBag = new ConcurrentBag<User>();
            foreach (User user in clients)
            {
                if (user != userToRemove)
                {
                    newBag.Add(user);
                }
            }

            clients = newBag;
        }

        private void HandleException(Exception ex, string additionalText = "")
        {
            SimpleLogs.WriteToFile($"[CommunicationWithClient.cs] {additionalText} " + ex.ToString());
        }

        private bool CloseConnection(TcpClient client, User currentUser)
        {
            try
            {
                RemoveItemFromConcurrentBag(currentUser);
                client.Close();

                Console.WriteLine("Client disconnected: " + currentUser.Id);
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
        private NetworkStream Stream { get; set; }

        public RSAGenerating rsaGeneratingServer;

        public RSAGenerating rsaGeneratingClient;

        public InteractWithClient(TcpClient client, RSAGenerating rsaClient, RSAGenerating rsaServer)
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
                message = rsaGeneratingServer.DecryptIntoString(buffer);

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
                byte[] buffer = rsaGeneratingClient.EncryptString(message);
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
