using ClientSide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class CommunicationWithServerTests
    {
        private TcpClient tcpClient;
        private BasicClient basicClientInfo;
        private CommunicationWithServer communicationWithServer;

        [SetUp]
        public void Setup()
        {
            tcpClient = new TcpClient();
            basicClientInfo = new BasicClient(new BasicInfoIpAddress(Constants.DebugMode, Constants.DefaultIP, Constants.DefaultPort));
            communicationWithServer = new CommunicationWithServer(tcpClient, basicClientInfo);
        }

        [TearDown]
        public void TearDown()
        {
            communicationWithServer.CloseConnection();
        }

        [Test]
        public async Task ConnectClientAsync_ClientConnected()
        { 
            // Act
            await communicationWithServer.ConnectClientAsync();

            // Assert
            Assert.IsTrue(tcpClient.Connected);
        }

        [Test]
        public async Task EstablishConnectionWithServer_ConnectionEstablished()
        {
            // Act
            await communicationWithServer.EstablishConnectionWithServer();

            // Assert
            Assert.IsTrue(communicationWithServer.EstablishedConnection);
            Assert.IsNotNull(communicationWithServer.interactWithServer);
        }

        [Test]
        public async Task InitialSetting_ServerPublicKeySet()
        {
            // Act
            await communicationWithServer.EstablishConnectionWithServer();
            await communicationWithServer.InitialSetting();

            // Assert
            Assert.IsNotNull(communicationWithServer.interactWithServer.rsaGeneratingServer.PublicKey);
        }

        [Test]
        public async Task GetRoomNamesFromServer_ReturnsRoomNames()
        {
            // Arrange
            string roomName = "testroom";
            string password = "password";

            // Act
            await communicationWithServer.EstablishConnectionWithServer();
            await communicationWithServer.InitialSetting();
            await communicationWithServer.SendCreateRoomAttemptToServer(roomName, password);
            string[] roomNames = await communicationWithServer.GetRoomNamesFromServer();

            // Assert
            Assert.IsNotNull(roomNames);
            Assert.IsNotEmpty(roomNames);
        }

        [Test]
        public async Task SetUsernameToServer_ValidUsername_ReturnsTrue()
        {
            // Arrange
            string username = "testuser";

            // Act
            await communicationWithServer.EstablishConnectionWithServer();
            await communicationWithServer.InitialSetting();
            bool result = await communicationWithServer.SetUsernameToServer(username);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task SendMessageFromClientToServer_MessageSentSuccessfully()
        {
            // Arrange
            string username = "testuser";
            string messageText = "Hello";

            // Act
            await communicationWithServer.EstablishConnectionWithServer();
            await communicationWithServer.InitialSetting();
            await communicationWithServer.SendMessageFromClientToServer(username, messageText);

            // Assert
            Assert.IsTrue(communicationWithServer.chatMessages.Count > 0);
            Message lastMessage = communicationWithServer.chatMessages[communicationWithServer.chatMessages.Count - 1];
            Assert.AreEqual(username, lastMessage.Username);
            Assert.AreEqual(messageText, lastMessage.Text);
        }

        [Test]
        public async Task GetMessageFromAnotherClient_ValidMessage_AddsMessageToChatMessages()
        {
            // Arrange
            string message = "[some_test];username;Hello";

            // Act
            await communicationWithServer.EstablishConnectionWithServer();
            await communicationWithServer.InitialSetting();
            communicationWithServer.GetMessageFromAnotherClient(message);

            // Assert
            Assert.IsTrue(communicationWithServer.chatMessages.Count > 0);
            Message lastMessage = communicationWithServer.chatMessages[communicationWithServer.chatMessages.Count - 1];
            Assert.AreEqual("username", lastMessage.Username);
            Assert.AreEqual("Hello, client!", lastMessage.Text);
        }

        [Test]
        public async Task SendJoinRoomAttemptToServer_ValidRoom_ReturnsTrue()
        {
            // Arrange
            string roomName = "testroom";
            string password = "password";

            // Act
            await communicationWithServer.EstablishConnectionWithServer();
            await communicationWithServer.InitialSetting();
            await communicationWithServer.SendCreateRoomAttemptToServer(roomName, password);
            bool result = await communicationWithServer.SendJoinRoomAttemptToServer(roomName, password);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task SendCreateRoomAttemptToServer_ValidRoom_ReturnsTrue()
        {
            // Arrange
            string roomName = "testroom";
            string password = "password";

            // Act
            await communicationWithServer.EstablishConnectionWithServer();
            await communicationWithServer.InitialSetting();
            bool result = await communicationWithServer.SendCreateRoomAttemptToServer(roomName, password);

            // Assert
            Assert.IsTrue(result);
        }
    }
}
