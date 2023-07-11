using ClientSide;
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

        /// <summary>
        /// Constructor set up client, basic client details, initializes a cancellation token source, and sets connection status flag
        /// </summary>
        /// <param name="client">TcpClient used for communication with server</param>
        /// <param name="basicClientInfo">BasicClient object containing basic client information</param>
        public CommunicationWithServer(TcpClient client, BasicClient basicClientInfo)
        {
            Client = client;
            BasicClientInfo = basicClientInfo;
            cancellationTokenSource = new CancellationTokenSource();

            EstablishedConnection = false;
        }

        /// <summary>
        /// Empty constructor
        /// </summary>
        public CommunicationWithServer() { }

        /// <summary>
        /// Connect client to server asynchrony 
        /// </summary>
        public async Task ConnectClientAsync()
        {
            await Client.ConnectAsync(BasicClientInfo.IpAddress, BasicClientInfo.Port);
        }

        /// <summary>
        /// Establishe connection with server asynchrony 
        /// </summary>
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

        /// <summary>
        /// Perform the initial setup asynchrony 
        /// </summary>
        public async Task InitialSettingAsync()
        {
            // Get server public key
            interactWithServer.rsaGeneratingServer.SetPublicKeyFromString(await interactWithServer.ReceiveMessageAsync());

            // Send client public key to server
            await interactWithServer.SendMessageAsync(interactWithServer.rsaGeneratingClient.PublicKey);

            // Get secure code from server
            SecureCode = await interactWithServer.ReceiveMessageWithEncryptionAsync();
        }

        /// <summary>
        /// Get the names of available chat rooms from server asynchrony 
        /// </summary>
        /// <returns>An array of room names</returns>
        public async Task<string[]> GetRoomNamesFromServerAsync()
        {
            await interactWithServer.SendMessageWithEncryptionAsync($"{SecureCode};{Constants.ServerMessageGetListOfRooms}");

            string roomNames = await interactWithServer.ReceiveMessageWithEncryptionAsync();

            // Get and compare the secure code. If it is not the same, close the connection
            if (!roomNames.Split(';')[0].Equals(SecureCode))
            {
                CloseConnection();
            }
            roomNames = string.Join(";", roomNames.Split(';').Skip(1));

            string[] arrayRoomNames = (roomNames.Split(';')).Skip(1).ToArray();

            return arrayRoomNames;
        }

        /// <summary>
        /// Start receiving messages from  server in a background task
        /// </summary>
        public void StartToReveiveMessages()
        {
            _ = Task.Run(() => ReceiveMessagesAsync(cancellationTokenSource.Token));
        }

        /// <summary>
        /// Set the username for client on server asynchrony 
        /// </summary>
        /// <param name="username">The username to set</param>
        /// <returns>True if the username was set successfully, otherwise false</returns>
        public async Task<bool> SetUsernameToServerAsync(string username)
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

        /// <summary>
        /// Send a message from client to server asynchrony 
        /// </summary>
        /// <param name="username">The username of client</param>
        /// <param name="messageText">The text of message</param>
        public async Task SendMessageFromClientToServerAsync(string username, string messageText)
        {
            chatMessages.Add(new Message(username, messageText, false));
            await interactWithServer.SendMessageWithEncryptionAsync($"{SecureCode};{Constants.ServerMessageSendMessage};{messageText}");
        }

        /// <summary>
        /// Get a message received from another client
        /// </summary>
        /// <param name="message">The received message</param>
        public void GetMessageFromAnotherClient(string message)
        {
            string[] elements = message.Split(';');
            string combinedString = string.Join(";", elements.Skip(2));

            chatMessages.Add(new Message(elements[1], combinedString, true));
        }

        /// <summary>
        /// Send a join room attempt to server asynchrony 
        /// </summary>
        /// <param name="roomName">The name of the room to join</param>
        /// <param name="password">The password for the room</param>
        /// <returns>True if the join attempt was successful, otherwise false</returns>
        public async Task<bool> SendJoinRoomAttemptToServerAsync(string roomName, string password)
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

        /// <summary>
        /// Send a create room attempt to server asynchrony 
        /// </summary>
        /// <param name="roomName">The name of the room to create</param>
        /// <param name="password">The password for the room</param>
        /// <returns>True if the create attempt was successful, false otherwise</returns>
        public async Task<bool> SendCreateRoomAttemptToServerAsync(string roomName, string password)
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

        /// <summary>
        /// Receive messages from server asynchrony 
        /// </summary>
        /// <param name="cancellationToken">Cancellation token for task cancellation</param>
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

        /// <summary>
        /// Close the connection with server
        /// </summary>
        /// <returns>True if the connection was closed successfully, otherwise false</returns>
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

        /// <summary>
        /// Send a message to server indicating client is closing the connection asynchrony 
        /// </summary>
        public async Task SendToServerMessageAboutClosingConnectionAsync()
        {
            await interactWithServer.SendMessageWithEncryptionAsync($"{SecureCode};{Constants.ServerMessageCloseConnection}");
        }

        /// <summary>
        /// Check if client is connected to server
        /// </summary>
        /// <returns>True if client is connected, otherwise false</returns>
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

        /// <summary>
        /// Constructor initialize a new instance of the InteractWithServer class
        /// </summary>
        /// <param name="stream">The NetworkStream used for communication with server</param>
        /// <param name="rsaClient">The RSAGenerating object for RSA encryption/decryption on client side</param>
        /// <param name="rsaServer">The RSAGenerating object for RSA encryption/decryption on server side</param>
        public InteractWithServer(NetworkStream stream, RSAGenerating rsaClient, RSAGenerating rsaServer)
        {
            Stream = stream;
            rsaGeneratingClient = rsaClient;
            rsaGeneratingServer = rsaServer;
        }

        /// <summary>
        /// Receive an encrypted message from server asynchrony
        /// </summary>
        /// <returns>The received decrypted message</returns>
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

        /// <summary>
        /// Receive a message from server asynchrony
        /// </summary>
        /// <returns>The received message</returns>
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

        /// <summary>
        /// Send an encrypted message to server asynchrony
        /// </summary>
        /// <param name="message">The message to send</param>
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

        /// <summary>
        /// Send a message to server asynchrony
        /// </summary>
        /// <param name="message">The message to send</param>
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

        /// <summary>
        /// Close the stream connection
        /// </summary>
        public void CloseStreamConnection()
        {
            Stream.Close();
        }
    }
}