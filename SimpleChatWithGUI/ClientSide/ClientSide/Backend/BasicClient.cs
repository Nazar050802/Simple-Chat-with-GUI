using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide
{
    internal class BasicClient
    {
        public IPAddress IpAddress { get; private set; }
        public int Port { get; private set; }  

        public BasicClient(BasicInfoIpAddress ipAddress)
        {
            // Get information about ip from object of class BasicInfoIpAddress
            IpAddress = ipAddress.GetIPAddress();
            Port = ipAddress.Port;
        }

        public TcpClient GetTcpClient()
        {
            // Return new TcpClient
            return new TcpClient();
        }

    }
}
