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
    internal class CommunicationWithClient
    {
        private ConcurrentBag<User> clients;
        private ConcurrentBag<Room> rooms;

        public CommunicationWithClient()
        {
            clients = new ConcurrentBag<User>();
            rooms = new ConcurrentBag<Room>();
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
                SimpleLogs.WriteToFile("[CommunicationWithClient.cs][ERROR] " + "Error occurred with client " + tempUserInfo.Id + ": " + ex.ToString());
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

                // Style of getting message: [TYPE];special_info;other_info...
                // Case: set username from user
                if (message.StartsWith(Constants.ServerMessageSetName))
                {
                    await GetAndSetUsernameFromClient(message, user, interactWithClient);
                }
                // Case: send list of rooms
                else if (message.StartsWith(Constants.ServerMessageGetListOfRooms))
                {
                    await SendRoomList(user, interactWithClient);
                }
                // Case: close connection
                else if (message.StartsWith(Constants.ServerMessageCloseConnection))
                {
                    break;
                }
                // Case: join to room
                else if (message.StartsWith(Constants.ServerMessageJoinToRoom))
                {
                    await JoinToRoom(user, interactWithClient, message);
                }
                // Case: create room
                else if (message.StartsWith(Constants.ServerMessageCreateRoom))
                {
                    await CreateRoom(user, interactWithClient, message);
                }
                // Case: get message
                else if (message.StartsWith(Constants.ServerMessageSendMessage))
                {
                    string[] elements = message.Split(';');
                    string message_to_broadcast = string.Join("", elements.Skip(1));
                    BroadcastMessage(message_to_broadcast, user);
                }

                SimpleLogs.WriteToFile("[CommunicationWithClient.cs][INFO] " + $"Received from {user.Id}({user.Name}): {message}");
                message = await interactWithClient.ReceiveMessageWithEncryptionAsync();
            }
        }

        public async Task GetAndSetUsernameFromClient(string message, User user, InteractWithClient interactWithClient)
        {
            string[] elements = message.Split(';');
            string combinedString = string.Join("", elements.Skip(1));

            string username = combinedString;
            if (clients.Any(user => user.Name == username))
            {
                await interactWithClient.SendMessageWithEncryptionAsync(Constants.ServerMessageWrongName);
            }
            else
            {
                await interactWithClient.SendMessageWithEncryptionAsync(Constants.ServerMessageSuccessName);

                User updatedUser = clients.FirstOrDefault(userToUpdate => userToUpdate.Id == user.Id);
                if (updatedUser != null)
                {
                    updatedUser.Name = username;
                }
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
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return new Tuple<User, InteractWithClient>(user, interactWithClient);
        }

        public async Task SendRoomList(User user, InteractWithClient interactWithClient)
        {
            await interactWithClient.SendMessageWithEncryptionAsync($"{Constants.ServerMessageGetListOfRooms};{string.Join(";", rooms.Select(room => room.Name))}");
        }

        public async Task JoinToRoom(User user, InteractWithClient interactWithClient, string message)
        {

            // Delete user from current room
            CloseRoom(user);

            string[] elements = message.Split(';');

            string roomName = elements[1];
            string password = elements[2];

            // Choose room
            Room foundRoom = rooms.FirstOrDefault(room => room.Name == roomName);

            if (foundRoom != null)
            {
                if (!foundRoom.ComparePassword(password))
                {
                    // Password incorrect send this information to user
                    await interactWithClient.SendMessageWithEncryptionAsync(Constants.ServerMessageWrongRoomPassword);
                }
                else
                {
                    // Everything is correct send this information to user
                    await interactWithClient.SendMessageWithEncryptionAsync(Constants.ServerMessageSuccessJoinToRoom);
                    user.CurrentRoomName = foundRoom.Name;
                    foundRoom.UsersInRoom.Add(user.Id);
                }
            }
        }

        public async Task CreateRoom(User user, InteractWithClient interactWithClient, string message)
        {

            string[] elements = message.Split(';');

            string roomName = elements[1];
            string password = elements[2];

            // Create room
            Room foundRoom = rooms.FirstOrDefault(room => room.Name == roomName);

            if (foundRoom != null)
            {
                // Room already exist send wrong message
                await interactWithClient.SendMessageWithEncryptionAsync(Constants.ServerMessageWrongCreateRoom);
            }
            else
            {
                // Create room using the login and password
                Room createRoom = new Room(roomName, password);
                user.CurrentRoomName = createRoom.Name;
                createRoom.UsersInRoom.Add(user.Id);

                rooms.Add(createRoom);

                await interactWithClient.SendMessageWithEncryptionAsync(Constants.ServerMessageSuccessCreateRoom);
            }
        }

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
                        _ = interactWithClient.SendMessageWithEncryptionAsync($"{Constants.ServerMessageGetMessage};{currentUser.Name};{message}");
                    }
                }
            }
        }

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


        private void HandleException(Exception ex, string additionalText = "")
        {
            SimpleLogs.WriteToFile($"[CommunicationWithClient.cs] {additionalText} " + ex.ToString());
        }

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
