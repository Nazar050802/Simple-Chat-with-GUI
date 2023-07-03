using System.Net;

namespace UnitTests
{
    [TestFixture]
    public class BasicInfoIpAddressTests
    {
        [Test]
        public void GetIPEndPoint_ReturnsCorrectIPEndPoint()
        {
            // Arrange
            bool debugMode = Constants.DebugMode;
            string ipAddress = Constants.DefaultIP;
            int port = Constants.DefaultPort;

            BasicInfoIpAddress ipAddressInfo = new BasicInfoIpAddress(debugMode, ipAddress, port);
            IPAddress expectedIPAddress = IPAddress.Parse(ipAddress);
            IPEndPoint expectedIPEndPoint = new IPEndPoint(expectedIPAddress, port);

            // Act
            IPEndPoint result = ipAddressInfo.GetIPEndPoint();

            // Assert
            Assert.AreEqual(expectedIPEndPoint.Address, result.Address);
            Assert.AreEqual(expectedIPEndPoint.Port, result.Port);
        }

        [Test]
        public void GetIPAddress_ReturnsCorrectIPAddress()
        {
            // Arrange
            bool debugMode = Constants.DebugMode;
            string ipAddress = Constants.DefaultIP;
            BasicInfoIpAddress ipAddressInfo = new BasicInfoIpAddress(debugMode, ipAddress);
            IPAddress expectedIPAddress = IPAddress.Parse(ipAddress);

            // Act
            IPAddress result = ipAddressInfo.GetIPAddress();

            // Assert
            Assert.AreEqual(expectedIPAddress, result);
        }
    }
}