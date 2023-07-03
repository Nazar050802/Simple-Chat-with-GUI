using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide
{
    public interface ISocketFactory
    {
        Socket CreateSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType);
    }

    // Class for Tcp protocol type of communication
    public class TcpSocketFactory : ISocketFactory
    {
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

        public BasicListener(BasicInfoIpAddress ipAddress, int backlog = 10) 
        {
            // Get information about ip from object of class BasicInfoIpAddress
            IpEndPoint = ipAddress.GetIPEndPoint();
            IpAddress = ipAddress.GetIPAddress();
            Backlog = backlog;  
        }

        public TcpListener GetTcpListener()
        {
            // Create new listener
            TcpListener listener = new TcpListener(IpEndPoint);

            return listener;
        }

    }
}
