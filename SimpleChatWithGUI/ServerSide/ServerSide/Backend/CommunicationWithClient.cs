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
using System.Xml.Linq;

namespace ServerSide
{
    public class CommunicationWithClient
    {
        private ConcurrentBag<User> clients;
        private ConcurrentBag<Room> rooms;

        /// <summary>
        /// Constructor initialize a new instance of the CommunicationWithClient class
        /// </summary>
        public CommunicationWithClient()
        {
            clients = new ConcurrentBag<User>();
            rooms = new ConcurrentBag<Room>();
        }

        /// <summary>
        /// Get the collection of connected clients
        /// </summary>
        /// <returns>The collection of connected clients</returns>
        public ConcurrentBag<User> GetClients()
        {
            return clients;
        }

        /// <summary>
        /// Get the collection of available rooms
        /// </summary>
        /// <returns>The collection of available rooms</returns>
        public ConcurrentBag<Room> GetRooms()
        {
            return rooms;
        }

        /// <summary>
        /// Handle the communication with a client asynchrony
        /// </summary>
        /// <param name="client">The TcpClient object representing the connected client</param>
        public async Task HandleClientAsync(TcpClient client)
        {
            User tempUserInfo = new User(client);

            InteractWithClient interactWithClient = new InteractWithClient(client, new RSAGenerating(), tempUserInfo.rsaGeneratingServer);

            Tuple<User, InteractWithClient> outputInitialSetting  = await InitialSettingAsync(tempUserInfo, interactWithClient);
            tempUserInfo = outputInitialSetting.Item1;
            interactWithClient = outputInitialSetting.Item2;

            clients.Add(tempUserInfo);

            try
            {
                await StartReceiveAndSendMessagesAsync(tempUserInfo, interactWithClient);
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[CommunicationWithClient.cs][ERROR] " + "Error occurred with client " + tempUserInfo.Id + ": " + ex.ToString());
                HandleException(ex);
            }
            finally
            {
                interactWithClient.CloseStreamConnection();
                CloseConnection(client, tempUserInfo);
            }
        }

        /// <summary>
        /// Start the process of receiving and transmitting messages between the client and server asynchrony
        /// </summary>
        /// <param name="user">User object representing the connected client</param>
        /// <param name="interactWithClient">InteractWithClient object for client communication</param>
        public async Task StartReceiveAndSendMessagesAsync(User user, InteractWithClient interactWithClient)
        {
            string message = await interactWithClient.ReceiveMessageWithEncryptionAsync();
            int counterForCloseConnection = 0;
            while (message != Constants.ServerMessageCloseConnection)
            {   
                if(message == "")
                {
                    // Emergency close the client if an error occurs
                    counterForCloseConnection += 1;
                    if (counterForCloseConnection > 100)
                    {
                        break;
                    }
                }

                // Get and compare the secure code. If it is not the same, close the connection.
                string[] tempStringElements = message.Split(';');
                message = string.Join(";", tempStringElements.Skip(1));

                if (!tempStringElements[0].Equals(user.SecureCode))
                {
                    break;
                }

                // Style of getting message: [TYPE];special_info;other_info...
                // Case: set username from user
                if (message.StartsWith(Constants.ServerMessageSetName))
                {
                    await GetAndSetUsernameFromClientAsync(message, user, interactWithClient);
                }
                // Case: send list of rooms
                else if (message.StartsWith(Constants.ServerMessageGetListOfRooms))
                {
                    await SendRoomListAsync(user, interactWithClient);
                }
                // Case: close connection
                else if (message.StartsWith(Constants.ServerMessageCloseConnection))
                {
                    break;
                }
                // Case: join to room
                else if (message.StartsWith(Constants.ServerMessageJoinToRoom))
                {
                    await JoinToRoomAsync(user, interactWithClient, message);
                }
                // Case: create room
                else if (message.StartsWith(Constants.ServerMessageCreateRoom))
                {
                    await CreateRoomAsync(user, interactWithClient, message);
                }
                // Case: get message
                else if (message.StartsWith(Constants.ServerMessageSendMessage))
                {
                    string[] elements = message.Split(';');
                    string message_to_broadcast = string.Join(";", elements.Skip(1));
                    BroadcastMessage(message_to_broadcast, user);
                }

                SimpleLogs.WriteToFile("[CommunicationWithClient.cs][INFO] " + $"Received from {user.Id}({user.Name}): {message}");
                message = await interactWithClient.ReceiveMessageWithEncryptionAsync();
            }
        }

        /// <summary>
        /// Manage the process of obtaining and setting the username from the client asynchrony
        /// </summary>
        /// <param name="message">Message that includes the username details</param>
        /// <param name="user">User object representing the connected client</param>
        /// <param name="interactWithClient">InteractWithClient object for client communication</param>
        public async Task GetAndSetUsernameFromClientAsync(string message, User user, InteractWithClient interactWithClient)
        {
            string[] elements = message.Split(';');
            string combinedString = string.Join("", elements.Skip(1));

            string username = combinedString;
            if (clients.Any(user => user.Name == username))
            {
                await interactWithClient.SendMessageWithEncryptionAsync($"{user.SecureCode};{Constants.ServerMessageWrongName}");
            }
            else
            {
                await interactWithClient.SendMessageWithEncryptionAsync($"{user.SecureCode};{Constants.ServerMessageSuccessName}");

                User updatedUser = clients.FirstOrDefault(userToUpdate => userToUpdate.Id == user.Id);
                if (updatedUser != null)
                {
                    updatedUser.Name = username;
                }
            }
        }

        /// <summary>
        /// Execute the initial setting with the client asynchrony
        /// </summary>
        /// <param name="user">The User object representing the connected client</param>
        /// <param name="interactWithClient">InteractWithClient object for client communication</param>
        /// <returns>Tuple containing the updated User and InteractWithClient objects</returns>
        public async Task<Tuple<User, InteractWithClient>> InitialSettingAsync(User user, InteractWithClient interactWithClient)
        {
            try
            {
                // Send server public key to client
                await interactWithClient.SendMessageAsync(user.rsaGeneratingServer.PublicKey);

                // Get client public key
                user.rsaGeneratingClient.SetPublicKeyFromString(await interactWithClient.ReceiveMessageAsync());
                interactWithClient.rsaGeneratingClient = user.rsaGeneratingClient;

                // Send Secure Code to the client
                await interactWithClient.SendMessageWithEncryptionAsync(user.SecureCode);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return new Tuple<User, InteractWithClient>(user, interactWithClient);
        }

        /// <summary>
        /// Send the list of available rooms to the client asynchrony
        /// </summary>
        /// <param name="user">User object representing the connected client</param>
        /// <param name="interactWithClient">InteractWithClient object for client communication</param>
        public async Task SendRoomListAsync(User user, InteractWithClient interactWithClient)
        {
            await interactWithClient.SendMessageWithEncryptionAsync($"{user.SecureCode};{Constants.ServerMessageGetListOfRooms};{string.Join(";", rooms.Select(room => room.Name))}");
        }

        /// <summary>
        /// Join client to room asynchrony
        /// </summary>
        /// <param name="user">The User object representing the connected client</param>
        /// <param name="interactWithClient">The InteractWithClient object for client communication</param>
        /// <param name="message">Message text containing the room name and password</param>
        public async Task JoinToRoomAsync(User user, InteractWithClient interactWithClient, string message)
        {

            string[] elements = message.Split(';');

            string roomName = elements[1];
            string password = elements[2];

            // Delete user from current room only if it's attempt another room
            if (user.CurrentRoomName != roomName)
            {
                CloseRoom(user);
            }
 
            // Choose room
            Room foundRoom = rooms.FirstOrDefault(room => room.Name == roomName);

            if (foundRoom != null)
            {
                if (!foundRoom.ComparePassword(password))
                {
                    // Password incorrect send this information to user
                    await interactWithClient.SendMessageWithEncryptionAsync($"{user.SecureCode};{Constants.ServerMessageWrongRoomPassword}");
                }
                else
                {
                    // Everything is correct send this information to user
                    await interactWithClient.SendMessageWithEncryptionAsync($"{user.SecureCode};{Constants.ServerMessageSuccessJoinToRoom}");
                    user.CurrentRoomName = foundRoom.Name;
                    foundRoom.UsersInRoom.Add(user.Id);
                }
            }
        }

        /// <summary>
        /// Manage the process of client room creation request asynchrony
        /// </summary>
        /// <param name="user">User object representing the connected client</param>
        /// <param name="interactWithClient">InteractWithClient object for client communication</param>
        /// <param name="message">Message text containing the room name and password</param>
        public async Task CreateRoomAsync(User user, InteractWithClient interactWithClient, string message)
        {

            string[] elements = message.Split(';');

            string roomName = elements[1];
            string password = elements[2];

            // Create room
            Room foundRoom = rooms.FirstOrDefault(room => room.Name == roomName);

            if (foundRoom != null)
            {
                // Room already exist send wrong message
                await interactWithClient.SendMessageWithEncryptionAsync($"{user.SecureCode};{Constants.ServerMessageWrongCreateRoom}");
            }
            else
            {
                // Create room using the login and password
                Room createRoom = new Room(roomName, password);
                user.CurrentRoomName = createRoom.Name;
                createRoom.UsersInRoom.Add(user.Id);

                rooms.Add(createRoom);

                await interactWithClient.SendMessageWithEncryptionAsync($"{user.SecureCode};{Constants.ServerMessageSuccessCreateRoom}");
            }
        }

        /// <summary>
        /// Send a message to all users in the same room as the sender
        /// </summary>
        /// <param name="message">Message to be broadcasted</param>
        /// <param name="currentUser">User object representing the message sender</param>
        public void BroadcastMessage(string message, User currentUser)
        {
            Room foundRoom = rooms.FirstOrDefault(room => room.Name == currentUser.CurrentRoomName);
            List<User> usersInRoom = new List<User>();

            if(foundRoom != null)
            {
                foreach (string userId in foundRoom.UsersInRoom)
                {
                    User tempUser = clients.FirstOrDefault(user => user.Id == userId);
                    usersInRoom.Add(tempUser);
                }

                foreach (User user in usersInRoom)
                {
                    if (user.Id != currentUser.Id)
                    {
                        InteractWithClient interactWithClient = new InteractWithClient(user.TcpConnection, user.rsaGeneratingClient, user.rsaGeneratingServer);
                        _ = interactWithClient.SendMessageWithEncryptionAsync($"{user.SecureCode};{Constants.ServerMessageGetMessage};{currentUser.Name};{message}");
                    }
                }
            }
        }

        /// <summary>
        /// Removes an item from a ConcurrentBag
        /// </summary>
        /// <typeparam name="T">The type of items in the bag</typeparam>
        /// <param name="itemToRemove">The item to be removed from the bag</param>
        /// <param name="oldBag">The original ConcurrentBag</param>
        /// <returns>New ConcurrentBag without the specified item</returns>
        private ConcurrentBag<T> RemoveItemFromConcurrentBag<T>(T itemToRemove, ConcurrentBag<T> oldBag)
        {
            ConcurrentBag<T> newBag = new ConcurrentBag<T>();
            foreach (T item in oldBag)
            {
                if(item != null)
                {
                    if (!item.Equals(itemToRemove))
                    {
                        newBag.Add(item);
                    }
                }
            }

            return newBag;
        }

        /// <summary>
        /// Manages exception and logs it to a file.
        /// </summary>
        /// <param name="ex">The exception to handle</param>
        /// <param name="additionalText">Additional text to include in the log file</param>
        private void HandleException(Exception ex, string additionalText = "")
        {
            SimpleLogs.WriteToFile($"[CommunicationWithClient.cs] {additionalText} " + ex.ToString());
        }

        /// <summary>
        /// Close the connection with a client
        /// </summary>
        /// <param name="client">TcpClient representing the client connection</param>
        /// <param name="currentUser">User object representing the current client</param>
        /// <returns>True if the connection was closed successfully, otherwise false</returns>
        private bool CloseConnection(TcpClient client, User currentUser)
        {
            try
            {
                clients = RemoveItemFromConcurrentBag<User>(currentUser, clients);
                client.Close();
                CloseRoom(currentUser);

                SimpleLogs.WriteToFile("[CommunicationWithClient.cs][INFO] " + "Client disconnected: " + currentUser.Id);
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[CommunicationWithClient.cs][ERROR] " + ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Close the room associated with the current user
        /// </summary>
        /// <param name="currentUser">The User object representing the current client</param>
        /// <returns>True if the room was closed successfully, otherwise false</returns>
        private bool CloseRoom(User currentUser)
        {
            try
            {
                Room foundRoom = rooms.FirstOrDefault(room => room.Name == currentUser.CurrentRoomName);
                if (foundRoom != null)
                {
                    foundRoom.UsersInRoom = RemoveItemFromConcurrentBag<string>(currentUser.Id, foundRoom.UsersInRoom);
                }
                else
                {
                    return false;
                }

                if (foundRoom.UsersInRoom.IsEmpty)
                {
                    rooms = RemoveItemFromConcurrentBag<Room>(foundRoom, rooms);
                }
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[CommunicationWithClient.cs][ERROR] " + ex.ToString());
                return false;
            }

            return true;
        }
    }

    public class InteractWithClient
    {
        private NetworkStream Stream { get; set; }

        public RSAGenerating rsaGeneratingServer { get; set; }

        public RSAGenerating rsaGeneratingClient { get; set; }

        /// <summary>
        /// Constructoir initialize a new instance of the InteractWithClient class
        /// </summary>
        /// <param name="client">The TcpClient representing the connected client</param>
        /// <param name="rsaClient">The RSAGenerating object for RSA encryption/decryption on client side</param>
        /// <param name="rsaServer">The RSAGenerating object for RSA encryption/decryption on server side</param>
        public InteractWithClient(TcpClient client, RSAGenerating rsaClient, RSAGenerating rsaServer)
        {
            Stream = client.GetStream();

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
                message = rsaGeneratingServer.DecryptIntoString(buffer);

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
                byte[] buffer = rsaGeneratingClient.EncryptString(message);
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
