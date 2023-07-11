using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClientSide
{
    public class BasicClient
    {
        public IPAddress IpAddress { get; private set; }
        public int Port { get; private set; }

        /// <summary>
        /// Constructor gets the IP address and port from the BasicInfoIpAddress object
        /// </summary>
        /// <param name="ipAddress">The BasicInfoIpAddress object containing the IP address and port information</param>
        public BasicClient(BasicInfoIpAddress ipAddress)
        {
            IpAddress = ipAddress.GetIPAddress();
            Port = ipAddress.Port;
        }

        /// <summary>
        /// Create and return a new instance of TcpClient
        /// </summary>
        /// <returns>A new instance of the TcpClient class</returns>
        public TcpClient GetTcpClient()
        {
            return new TcpClient();
        }

    }
}
