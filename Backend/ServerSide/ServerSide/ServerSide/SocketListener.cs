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

    internal class SocketListener
    {
        private IPEndPoint IpEndPoint { get; set; }
        private IPAddress IpAddress { get; set; }
        private int Backlog { get; set; }

        public SocketListener(BasicInfoIpAddress ipAddress, int backlog = 10) 
        {
            // Get information about ip from object of class BasicInfoIpAddress
            IpEndPoint = ipAddress.GetIPEndPoint();
            IpAddress = ipAddress.GetIPAddress();
            Backlog = backlog;  
        }

        public Socket GetTcpListener()
        {
            // Create new listener
            Socket sListener = new TcpSocketFactory().CreateSocket(IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sListener.Bind(IpEndPoint);
            sListener.Listen(Backlog);

            return sListener;
        }

    }
}
