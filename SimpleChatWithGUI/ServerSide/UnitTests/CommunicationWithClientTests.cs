using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class CommunicationWithClientTests
    {
        private CommunicationWithClient communicationWithClient;
        private BasicListener sListener;
        private TcpListener listener;

        [SetUp]
        public void SetUp()
        {
            communicationWithClient = new CommunicationWithClient();
            sListener = new BasicListener(new BasicInfoIpAddress(Constants.DebugMode, Constants.DefaultIP, Constants.DefaultPort));
            listener = sListener.GetTcpListener();
            listener.Start();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            listener.Stop();
        }

        [Test]
        public async Task Test_HandleClientAsync()
        {
            // Arrange
            TcpClient client = await listener.AcceptTcpClientAsync();

            // Act
            _ = communicationWithClient.HandleClientAsync(client);

            // Assert
            Assert.IsTrue(client.Connected);
        }

        [Test]
        public async Task HandleClientAsync_WhenClientMessageIsSetName_CallsGetAndSetUsernameFromClient()
        {
            // Arrange
            TcpClient client = await listener.AcceptTcpClientAsync();
            _ = communicationWithClient.HandleClientAsync(client);

            // Act
            // Time for manual testing. What to do:
            // 1. Just enter username "test" in the program
            await Task.Delay(10000);

            // Assert
            Assert.IsTrue(communicationWithClient.GetClients().Count > 0);
            Assert.IsTrue(communicationWithClient.GetClients().TryPeek(out var lastUser) && lastUser?.Name == "test");

            // Close program window, start next test in the new window
        }

        [Test]
        public async Task StartReceiveAndSendMessagesAsync_WhenMessageIsCreateRoom_CallsCreateRoom()
        {
            // Arrange
            TcpClient client = await listener.AcceptTcpClientAsync();
            _ = communicationWithClient.HandleClientAsync(client);

            // Act
            // Time for manual testing. What to do:
            // 1. Enter username in the program
            // 2. Create room with the name "test" and password "password"
            await Task.Delay(15000); 

            // Assert
            Assert.IsTrue(communicationWithClient.GetRooms().Count > 0);
            Assert.IsTrue(communicationWithClient.GetRooms().TryPeek(out var lastRoom) && (lastRoom?.Name == "test" && lastRoom.ComparePassword("password")));

            // Close program window, start next test in the new window
        }

        [Test]
        public async Task JoinToRoom_WhenRoomExistsAndPasswordIsCorrect_JoinsToRoom()
        {
            // Time for manual testing. What to do:
            // Creating room with the name "test" and password "password"
            TcpClient client1 = await listener.AcceptTcpClientAsync();
            _ = communicationWithClient.HandleClientAsync(client1);

            // Open the second program window
            TcpClient client2 = await listener.AcceptTcpClientAsync();
            _ = communicationWithClient.HandleClientAsync(client2);

            // Act
            // Time for manual testing. What to do:
            // 1. Enter username in the program
            // 2. Join to room with the name "test" and password "password"
            await Task.Delay(25000);
            
            // Assert
            Assert.IsTrue(communicationWithClient.GetRooms().Count > 0);
            Assert.IsTrue(communicationWithClient.GetRooms().TryPeek(out var lastRoom) && lastRoom.UsersInRoom.Count > 1);

            // Close program window, start next test in the new window
        }
    }
}
