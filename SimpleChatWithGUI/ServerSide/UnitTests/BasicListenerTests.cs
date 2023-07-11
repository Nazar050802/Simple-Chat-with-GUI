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
    public class BasicListenerTests
    {
        /// <summary>
        /// Test to check the GetTcpListener method of BasicListener class by verifying
        /// that the method returns a TcpListener with the correct IP endpoint.
        /// </summary>
        [Test]
        public void GetTcpListener_ReturnsTcpListenerWithCorrectEndPoint()
        {
            string ipAddress = Constants.DefaultIP;
            int port = Constants.DefaultPort;

            // Arrange
            BasicInfoIpAddress basicInfoIpAddress = new BasicInfoIpAddress(Constants.DebugMode, ipAddress, port);
            BasicListener basicListener = new BasicListener(basicInfoIpAddress);

            // Act
            TcpListener tcpListener = basicListener.GetTcpListener();

            // Assert
            Assert.AreEqual(IPAddress.Parse(ipAddress), ((IPEndPoint)tcpListener.LocalEndpoint).Address);
            Assert.AreEqual(port, ((IPEndPoint)tcpListener.LocalEndpoint).Port);
        }
    }
}
