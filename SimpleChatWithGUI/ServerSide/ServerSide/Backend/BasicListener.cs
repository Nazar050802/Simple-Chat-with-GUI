using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide
{
    /// <summary>
    /// Interface for a socket factory.
    /// </summary>
    public interface ISocketFactory
    {
        Socket CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType);
    }

    public class TcpSocketFactory : ISocketFactory
    {
        /// <summary>
        /// Create a new socket based on the supplied address family, socket type, and protocol type
        /// </summary>
        /// <param name="addressFamily">Address family for the socket</param>
        /// <param name="socketType">Type of socket to create</param>
        /// <param name="protocolType">Protocol type for the socket</param>
        /// <returns>The newly created socket</returns>
        public Socket CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType)
        {
            return new Socket(addressFamily, socketType, protocolType);
        }
    }

    public class BasicListener
    {
        private IPEndPoint IpEndPoint { get; set; }
        private IPAddress IpAddress { get; set; }
        public int Backlog { get; set; }

        /// <summary>
        /// Initialize a new instance of BasicListener
        /// </summary>
        /// <param name="ipAddress">Information about the IP address</param>
        /// <param name="backlog">The maximum length of the queue of pending connections</param>
        public BasicListener(BasicInfoIpAddress ipAddress, int backlog = 10) 
        {
            // Get information about ip from object of class BasicInfoIpAddress
            IpEndPoint = ipAddress.GetIPEndPoint();
            IpAddress = ipAddress.GetIPAddress();
            Backlog = backlog;  
        }

        /// <summary>
        /// Generate a new TcpListener based on the IP endpoint information
        /// </summary>
        /// <returns>The created TcpListener</returns>
        public TcpListener GetTcpListener()
        {
            // Create new listener
            TcpListener listener = new TcpListener(IpEndPoint);

            return listener;
        }

    }
}
