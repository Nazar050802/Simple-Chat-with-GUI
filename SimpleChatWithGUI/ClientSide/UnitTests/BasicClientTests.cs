using System.Net;
using System.Net.Sockets;

namespace UnitTests
{
    public class BasicClientTests
    {

        private BasicInfoIpAddress ipAddress;

        [SetUp]
        public void SetUp()
        {
            // Arrange
            string ipAddressSetUp = Constants.DefaultIP; 
            int portSetUp = Constants.DefaultPort;
            bool debugModeSetUp = Constants.DebugMode;

            ipAddress = new BasicInfoIpAddress(debugModeSetUp, ipAddressSetUp, portSetUp);
        }

        [Test]
        public void Constructor_InitializesIpAddressAndPort()
        {
            // Arrange
            string ipAddressEquals = Constants.DefaultIP;
            int portEquals = Constants.DefaultPort;

            // Act
            BasicClient client = new BasicClient(ipAddress);

            // Assert
            Assert.AreEqual(IPAddress.Parse(ipAddressEquals), client.IpAddress);
            Assert.AreEqual(portEquals, client.Port);
        }

        [Test]
        public void GetTcpClient_ReturnsTcpClientInstance()
        {
            // Arrange
            BasicClient client = new BasicClient(ipAddress);

            // Act
            TcpClient tcpClient = client.GetTcpClient();

            // Assert
            Assert.NotNull(tcpClient);
            Assert.IsInstanceOf<TcpClient>(tcpClient);
        }

    }
}
