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
            while (message != "")
            {
                SimpleLogs.WriteToFile("[CommunicationWithClient.cs][INFO] " + $"Received from {user.Id}({user.Name}): {message}");

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

                // User select or create room
                await SelectRoom(user, interactWithClient);
                
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return new Tuple<User, InteractWithClient>(user, interactWithClient);
        }

        public async Task SelectRoom(User user, InteractWithClient interactWithClient)
        {
            // Send room list
            await interactWithClient.SendMessageWithEncryptionAsync(string.Join(",", rooms.Select(room => room.Name)));

            // Choose or create room
            string[] choosenRoomNameAndPassword = (await interactWithClient.ReceiveMessageWithEncryptionAsync()).Split(',');
            Room foundRoom = rooms.FirstOrDefault(room => room.Name == choosenRoomNameAndPassword[0]);

            string password = choosenRoomNameAndPassword[1];
            if (foundRoom != null)
            {
                while (!foundRoom.ComparePassword(password))
                {
                    // Send message that password is wrong
                    await interactWithClient.SendMessageWithEncryptionAsync(Constants.ServerMessageWrongRoomPassword);

                    // Get new data
                    password = (await interactWithClient.ReceiveMessageWithEncryptionAsync()).Split(',')[1];
                }

                user.CurrentRoomName = foundRoom.Name;
                foundRoom.UsersInRoom.Add(user.Id);
            }
            else
            {
                // Create room using the login and password
                Room createRoom = new Room(choosenRoomNameAndPassword[0], choosenRoomNameAndPassword[1]);
                user.CurrentRoomName = createRoom.Name;
                createRoom.UsersInRoom.Add(user.Id);

                rooms.Add(createRoom);
            }

            // Send message that everything was success
            await interactWithClient.SendMessageWithEncryptionAsync(Constants.ServerMessageSuccessJoinToRoom);
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
                        _ = interactWithClient.SendMessageWithEncryptionAsync(message);
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
