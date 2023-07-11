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

        /// <summary>
        /// Setup method for the test fixture
        /// </summary>
        [SetUp]
        public void Setup()
        {
            tcpClient = new TcpClient();
            basicClientInfo = new BasicClient(new BasicInfoIpAddress(Constants.DebugMode, Constants.DefaultIP, Constants.DefaultPort));
            communicationWithServer = new CommunicationWithServer(tcpClient, basicClientInfo);
        }

        /// <summary>
        /// Tear down method for the test fixture
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            communicationWithServer.CloseConnection();
        }

        /// <summary>
        /// Test to check that ConnectClientAsync method connects the client to the server
        /// </summary>
        [Test]
        public async Task ConnectClientAsync_ClientConnected()
        { 
            // Act
            await communicationWithServer.ConnectClientAsync();

            // Assert
            Assert.IsTrue(tcpClient.Connected);
        }

        /// <summary>
        /// Test to check that EstablishConnectionWithServer method establishes a connection with the server
        /// </summary>
        [Test]
        public async Task EstablishConnectionWithServer_ConnectionEstablished()
        {
            // Act
            await communicationWithServer.EstablishConnectionWithServer();

            // Assert
            Assert.IsTrue(communicationWithServer.EstablishedConnection);
            Assert.IsNotNull(communicationWithServer.interactWithServer);
        }

        /// <summary>
        /// Test to check that InitialSettingAsync method sets the server public key
        /// </summary>
        [Test]
        public async Task InitialSetting_ServerPublicKeySet()
        {
            // Act
            await communicationWithServer.EstablishConnectionWithServer();
            await communicationWithServer.InitialSettingAsync();

            // Assert
            Assert.IsNotNull(communicationWithServer.interactWithServer.rsaGeneratingServer.PublicKey);
        }

        /// <summary>
        /// Test to check that GetRoomNamesFromServerAsync method returns room names
        /// </summary>
        [Test]
        public async Task GetRoomNamesFromServer_ReturnsRoomNames()
        {
            // Arrange
            string roomName = "testroom";
            string password = "password";

            // Act
            await communicationWithServer.EstablishConnectionWithServer();
            await communicationWithServer.InitialSettingAsync();
            await communicationWithServer.SendCreateRoomAttemptToServerAsync(roomName, password);
            string[] roomNames = await communicationWithServer.GetRoomNamesFromServerAsync();

            // Assert
            Assert.IsNotNull(roomNames);
            Assert.IsNotEmpty(roomNames);
        }

        /// <summary>
        /// Test to check that SetUsernameToServerAsync method sets the username on the server and returns true
        /// </summary>
        [Test]
        public async Task SetUsernameToServer_ValidUsername_ReturnsTrue()
        {
            // Arrange
            string username = "testuser";

            // Act
            await communicationWithServer.EstablishConnectionWithServer();
            await communicationWithServer.InitialSettingAsync();
            bool result = await communicationWithServer.SetUsernameToServerAsync(username);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test to check that SendMessageFromClientToServerAsync method sends a message successfully
        /// </summary>
        [Test]
        public async Task SendMessageFromClientToServer_MessageSentSuccessfully()
        {
            // Arrange
            string username = "testuser";
            string messageText = "Hello";

            // Act
            await communicationWithServer.EstablishConnectionWithServer();
            await communicationWithServer.InitialSettingAsync();
            await communicationWithServer.SendMessageFromClientToServerAsync(username, messageText);

            // Assert
            Assert.IsTrue(communicationWithServer.chatMessages.Count > 0);
            Message lastMessage = communicationWithServer.chatMessages[communicationWithServer.chatMessages.Count - 1];
            Assert.AreEqual(username, lastMessage.Username);
            Assert.AreEqual(messageText, lastMessage.Text);
        }

        /// <summary>
        /// Test to check that GetMessageFromAnotherClient method adds a message to chat messages
        /// </summary>
        [Test]
        public async Task GetMessageFromAnotherClient_ValidMessage_AddsMessageToChatMessages()
        {
            // Arrange
            string message = "[some_test];username;Hello, world!";

            // Act
            await communicationWithServer.EstablishConnectionWithServer();
            await communicationWithServer.InitialSettingAsync();
            communicationWithServer.GetMessageFromAnotherClient(message);

            // Assert
            Assert.IsTrue(communicationWithServer.chatMessages.Count > 0);
            Message lastMessage = communicationWithServer.chatMessages[communicationWithServer.chatMessages.Count - 1];
            Assert.AreEqual("username", lastMessage.Username);
            Assert.AreEqual("Hello, world!", lastMessage.Text);
        }

        /// <summary>
        /// Test to check that SendJoinRoomAttemptToServerAsync method sends a valid room join request and returns true
        /// </summary>
        [Test]
        public async Task SendJoinRoomAttemptToServer_ValidRoom_ReturnsTrue()
        {
            // Arrange
            string roomName = "testroom";
            string password = "password";

            // Act
            await communicationWithServer.EstablishConnectionWithServer();
            await communicationWithServer.InitialSettingAsync();
            await communicationWithServer.SendCreateRoomAttemptToServerAsync(roomName, password);
            bool result = await communicationWithServer.SendJoinRoomAttemptToServerAsync(roomName, password);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test to check that SendCreateRoomAttemptToServerAsync method sends a valid room creation request and returns true
        /// </summary>
        [Test]
        public async Task SendCreateRoomAttemptToServer_ValidRoom_ReturnsTrue()
        {
            // Arrange
            string roomName = "testroom";
            string password = "password";

            // Act
            await communicationWithServer.EstablishConnectionWithServer();
            await communicationWithServer.InitialSettingAsync();
            bool result = await communicationWithServer.SendCreateRoomAttemptToServerAsync(roomName, password);

            // Assert
            Assert.IsTrue(result);
        }
    }
}
