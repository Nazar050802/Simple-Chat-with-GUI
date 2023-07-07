using ServerSide;
using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class CommunicationWithServer
    {
        private TcpClient Client { get; set; }
        private CancellationTokenSource cancellationTokenSource {get; set;}
        private BasicClient BasicClientInfo { get; set; }

        public InteractWithServer interactWithServer { get; set; }

        public bool EstablishedConnection { get; set; }

        public ObservableCollection<Message> chatMessages { get; set; } = new ObservableCollection<Message>();

        private string SecureCode { get; set; }

        public CommunicationWithServer(TcpClient client, BasicClient basicClientInfo)
        {
            Client = client;
            BasicClientInfo = basicClientInfo;
            cancellationTokenSource = new CancellationTokenSource();

            EstablishedConnection = false;
        }

        public CommunicationWithServer() { }

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

            EstablishedConnection = true;
            interactWithServer = new InteractWithServer(Client.GetStream(), new RSAGenerating(), new RSAGenerating());
        }

        public async Task InitialSetting()
        {
            // Get server public key
            interactWithServer.rsaGeneratingServer.SetPublicKeyFromString(await interactWithServer.ReceiveMessageAsync());

            // Send client public key to server
            await interactWithServer.SendMessageAsync(interactWithServer.rsaGeneratingClient.PublicKey);

            // Get secure code from server
            SecureCode = await interactWithServer.ReceiveMessageWithEncryptionAsync();
        }

        public async Task<string[]> GetRoomNamesFromServer()
        {
            await interactWithServer.SendMessageWithEncryptionAsync($"{SecureCode};{Constants.ServerMessageGetListOfRooms}");

            string roomNames = await interactWithServer.ReceiveMessageWithEncryptionAsync();

            // Get and compare the secure code. If it is not the same, close the connection.
            if (!roomNames.Split(';')[0].Equals(SecureCode))
            {
                CloseConnection();
            }
            roomNames = string.Join(";", roomNames.Split(';').Skip(1));

            string[] arrayRoomNames = (roomNames.Split(';')).Skip(1).ToArray();

            return arrayRoomNames;
        }

        public void StartToReveiveMessages()
        {
            _ = Task.Run(() => ReceiveMessagesAsync(cancellationTokenSource.Token));
        }

        public async Task<bool> SetUsernameToServer(string username)
        {
            try
            {
                await interactWithServer.SendMessageWithEncryptionAsync($"{SecureCode};{Constants.ServerMessageSetName};{username}");
                string response = await interactWithServer.ReceiveMessageWithEncryptionAsync();

                // Get and compare the secure code. If it is not the same, close the connection.
                if (!response.Split(';')[0].Equals(SecureCode))
                {
                    CloseConnection();
                }
                response = string.Join(";", response.Split(';').Skip(1));

                if (response != Constants.ServerMessageSuccessName)
                {
                    return false;
                }
            }
            catch (Exception ex) {
                SimpleLogs.WriteToFile("[CommunicationWithServer.cs][ERROR] " + ex.ToString());
                return false;
            }
            
            return true;
        }

        public async Task SendMessageFromClientToServer(string username, string messageText)
        {
            chatMessages.Add(new Message(username, messageText, false));
            await interactWithServer.SendMessageWithEncryptionAsync($"{SecureCode};{Constants.ServerMessageSendMessage};{messageText}");
        }

        public void GetMessageFromAnotherClient(string message)
        {
            string[] elements = message.Split(';');
            string combinedString = string.Join(";", elements.Skip(2));

            chatMessages.Add(new Message(elements[1], combinedString, true));
        }

        public async Task<bool> SendJoinRoomAttemptToServer(string roomName, string password)
        {
            try
            {
                await interactWithServer.SendMessageWithEncryptionAsync($"{SecureCode};{Constants.ServerMessageJoinToRoom};{roomName};{password}");
                string response = await interactWithServer.ReceiveMessageWithEncryptionAsync();

                // Get and compare the secure code. If it is not the same, close the connection.
                if (!response.Split(';')[0].Equals(SecureCode))
                {
                    CloseConnection();
                }
                response = string.Join(";", response.Split(';').Skip(1));

                if (response != Constants.ServerMessageSuccessJoinToRoom)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[CommunicationWithServer.cs][ERROR] " + ex.ToString());
                return false;
            }

            return true;
        }

        public async Task<bool> SendCreateRoomAttemptToServer(string roomName, string password)
        {
            try
            {
                await interactWithServer.SendMessageWithEncryptionAsync($"{SecureCode};{Constants.ServerMessageCreateRoom};{roomName};{password}");
                string response = await interactWithServer.ReceiveMessageWithEncryptionAsync();

                // Get and compare the secure code. If it is not the same, close the connection.
                if (!response.Split(';')[0].Equals(SecureCode))
                {
                    CloseConnection();
                }
                response = string.Join(";", response.Split(';').Skip(1));

                if (response != Constants.ServerMessageSuccessCreateRoom)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[CommunicationWithServer.cs][ERROR] " + ex.ToString());
                return false;
            }

            return true;
        }

        public async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
        {
            try
            {
                string message = await interactWithServer.ReceiveMessageWithEncryptionAsync();

                while (!cancellationToken.IsCancellationRequested && message != "")
                {
                    // Get and compare the secure code. If it is not the same, close the connection.
                    if (!message.Split(';')[0].Equals(SecureCode))
                    {
                        CloseConnection();
                    }
                    message = string.Join(";", message.Split(';').Skip(1));

                    // Style of getting message: [TYPE];special_info;other_info...
                    if (message.StartsWith(Constants.ServerMessageGetMessage))
                    {
                        GetMessageFromAnotherClient(message);
                    }
                    
                    message = await interactWithServer.ReceiveMessageWithEncryptionAsync();
                }
                
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[CommunicationWithServer.cs][ERROR] " + ex.ToString());
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

        public async Task SendToServerMessageAboutClosingConnection()
        {
            await interactWithServer.SendMessageWithEncryptionAsync($"{SecureCode};{Constants.ServerMessageCloseConnection}");
        }

        private bool IsClientConnected()
        {
            return Client.Connected;
        }

    }

    public class InteractWithServer
    {
        private NetworkStream Stream { get; set; }

        public RSAGenerating rsaGeneratingServer;

        public RSAGenerating rsaGeneratingClient;

        public InteractWithServer(NetworkStream stream, RSAGenerating rsaClient, RSAGenerating rsaServer)
        {
            Stream = stream;
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