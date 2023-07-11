using ClientSide;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace ClientSide
{
    class Controller
    {
        public ObservableCollection<Message> chatMessagesOld = new ObservableCollection<Message>();
        public ObservableCollection<Message> chatMessagesNew = new ObservableCollection<Message>();

        public ObservableCollection<string> RoomsNames = new ObservableCollection<string>();
        public string CurrentRoomName { get; set; } = "";
        private string Username { get; set; } = "";

        public ClientCore ClientCore { get; set; }

        /// <summary>
        /// Constructor initialize a new Controller instance
        /// </summary>
        /// <param name="ip">The server IP address. Default is Constants.DefaultIP</param>
        /// <param name="port">The server port number. Default is Constants.DefaultPort</param>
        public Controller(string ip = Constants.DefaultIP, int port = Constants.DefaultPort)
        {
            ClientCore = new ClientCore(ip, port);
        }

        /// <summary>
        /// Attempt to assign a username to the client
        /// </summary>
        /// <param name="nickname">The desired nickname</param>
        /// <returns>True if the nickname was set successfully, otherwise false</returns>
        public async Task<bool> TryToSetUsername(string nickname)
        {
            bool output = true;
            try
            {
                output = await ClientCore.communicationWithServer.SetUsernameToServerAsync(nickname);
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[Controller.cs][ERROR] " + ex.ToString());
                return false;
            }

            Username = nickname;

            return output;
        }

        /// <summary>
        /// Attempt to join a specified room on the server
        /// </summary>
        /// <param name="roomName">The room name to join</param>
        /// <param name="password">The room password to join</param>
        /// <returns>True if the room was joined successfully, otherwise false</returns>
        public async Task<bool> TryToJoinRoom(string roomName, string password)
        {
            bool output = true;
            try
            {
                CurrentRoomName = roomName;
                output = await ClientCore.communicationWithServer.SendJoinRoomAttemptToServerAsync(roomName, password);
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[Controller.cs][ERROR] " + ex.ToString());
                return false;
            }

            return output;
        }

        /// <summary>
        /// Attempt to create a new room on the server
        /// </summary>
        /// <param name="roomName">The room name to create</param>
        /// <param name="password">The room password to create</param>
        /// <returns>True if the room was created successfully, otherwise false</returns>
        public async Task<bool> TryToCreateRoom(string roomName, string password)
        {
            bool output = true;
            try
            {
                CurrentRoomName = roomName;
                output = await ClientCore.communicationWithServer.SendCreateRoomAttemptToServerAsync(roomName, password);
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[Controller.cs][ERROR] " + ex.ToString());
                return false;
            }

            return output;
        }

        /// <summary>
        /// Update the new chat messages collection asynchrony 
        /// </summary>
        public async Task SetNewMessageAsync()
        {
            chatMessagesNew = new ObservableCollection<Message>(ClientCore.communicationWithServer.chatMessages);
        }

        /// <summary>
        /// Send a message to server asynchrony
        /// </summary>
        /// <param name="messageText">The message text to send</param>
        public async Task SendMessageAsync(string messageText)
        {
            _ = ClientCore.communicationWithServer.SendMessageFromClientToServerAsync(Username, messageText);
        }

        /// <summary>
        /// Update the list of room names asynchrony
        /// </summary>
        public async Task UpdateRoomsNamesAsync()
        {
            RoomsNames = new ObservableCollection<string>(await ClientCore.communicationWithServer.GetRoomNamesFromServerAsync());
        }

        /// <summary>
        /// Refresh the chat messages collection
        /// </summary>
        public void VariableUpdateChatMessages()
        {
            chatMessagesOld = new ObservableCollection<Message>(chatMessagesNew);
        }

        /// <summary>
        /// Obtain new messages by comparing message IDs
        /// </summary>
        /// <returns>Collection of new messages</returns>
        public ObservableCollection<Message> GetNewMessages()
        {
            // Get new messages by comparing messages id
            var oldMessageIds = chatMessagesOld.Select(message => message.Id).ToList();

            var newMessages = chatMessagesNew.Where(message => !oldMessageIds.Contains(message.Id));

            return new ObservableCollection<Message>(newMessages);
        }

        /// <summary>
        /// Check if the client is currently connected to the server
        /// </summary>
        /// <returns>True if the client is connected to the server, otherwise false</returns>
        public bool IsClientConnetToTheServer()
        {
            return ClientCore.communicationWithServer.EstablishedConnection;
        }

        /// <summary>
        /// Initiate communication with the server
        /// </summary>
        public void StartCommunicate()
        {
            _ = ClientCore.StartCommunicateWithServerAsync();
        }

        /// <summary>
        /// Begin the process of receiving messages from other users
        /// </summary>
        public void StartReceiveMessagesFromUsers()
        {
            ClientCore.communicationWithServer.StartToReveiveMessages();
        }

        /// <summary>
        /// End communication with the server
        /// </summary>
        public void CloseCommunication()
        {
            _ = ClientCore.communicationWithServer.SendToServerMessageAboutClosingConnectionAsync();
            ClientCore.communicationWithServer.CloseConnection();
        }
    }
}
