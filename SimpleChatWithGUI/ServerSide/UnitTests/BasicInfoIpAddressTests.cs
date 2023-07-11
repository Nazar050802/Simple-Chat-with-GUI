using System.Net;

namespace UnitTests
{
    [TestFixture]
    public class BasicInfoIpAddressTests
    {
        /// <summary>
        /// Test to check the GetIPEndPoint method of BasicInfoIpAddress class by verifying
        /// that the method returns the correct IPEndPoint based on the provided IP address and port
        /// </summary>
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

        /// <summary>
        /// Test to check the GetIPAddress method of BasicInfoIpAddress class by verifying
        /// that the method returns the correct IPAddress based on the provided IP address string
        /// </summary>
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