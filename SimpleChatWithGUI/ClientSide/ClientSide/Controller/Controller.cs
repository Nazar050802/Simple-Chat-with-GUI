using ServerSide;
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

        public Controller(string ip = Constants.DefaultIP, int port = Constants.DefaultPort)
        {
            ClientCore = new ClientCore(ip, port);
        }

        public async Task<bool> TryToSetUsername(string nickname)
        {
            bool output = true;
            try
            {
                output = await ClientCore.communicationWithServer.SetUsernameToServer(nickname);
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[Controller.cs][ERROR] " + ex.ToString());
                return false;
            }

            Username = nickname;

            return output;
        }

        public async Task<bool> TryToJoinRoom(string roomName, string password)
        {
            bool output = true;
            try
            {
                CurrentRoomName = roomName;
                output = await ClientCore.communicationWithServer.SendJoinRoomAttemptToServer(roomName, password);
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[Controller.cs][ERROR] " + ex.ToString());
                return false;
            }

            return output;
        }

        public async Task<bool> TryToCreateRoom(string roomName, string password)
        {
            bool output = true;
            try
            {
                CurrentRoomName = roomName;
                output = await ClientCore.communicationWithServer.SendCreateRoomAttemptToServer(roomName, password);
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[Controller.cs][ERROR] " + ex.ToString());
                return false;
            }

            return output;
        }

        public async Task SetNewMessage()
        {
            chatMessagesNew = new ObservableCollection<Message>(ClientCore.communicationWithServer.chatMessages);
        }

        public async Task SendMessage(string messageText)
        {
            _ = ClientCore.communicationWithServer.SendMessageFromClientToServer(Username, messageText);
        }

        public async Task UpdateRoomsNames()
        {
            RoomsNames = new ObservableCollection<string>(await ClientCore.communicationWithServer.GetRoomNamesFromServer());
        }

        public void VariableUpdateChatMessages()
        {
            chatMessagesOld = new ObservableCollection<Message>(chatMessagesNew);
        }

        public ObservableCollection<Message> GetNewMessages()
        {
            // Get new messages by comparing messages id
            var oldMessageIds = chatMessagesOld.Select(message => message.Id).ToList();

            var newMessages = chatMessagesNew.Where(message => !oldMessageIds.Contains(message.Id));

            return new ObservableCollection<Message>(newMessages);
        }

        public bool IsClientConnetToTheServer()
        {
            return ClientCore.communicationWithServer.EstablishedConnection;
        }

        public void StartCommunicate()
        {
            _ = ClientCore.StartCommunicateWithServer();
        }

        public void StartReceiveMessagesFromUsers()
        {
            ClientCore.communicationWithServer.StartToReveiveMessages();
        }

        public void CloseCommunication()
        {
            _ = ClientCore.communicationWithServer.SendToServerMessageAboutClosingConnection();
            ClientCore.communicationWithServer.CloseConnection();
        }
    }
}
