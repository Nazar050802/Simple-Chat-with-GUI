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

    internal class SocketSender
    {
        private IPAddress IpAddress { get; set; }

        public SocketSender(BasicInfoIpAddress ipAddress)
        {
            // Get information about ip from object of class BasicInfoIpAddress
            IpAddress = ipAddress.GetIPAddress();
        }

        public Socket GetTcpSender()
        {
            // Create new Sender
            Socket sender = new TcpSocketFactory().CreateSocket(IpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            return sender;
        }

    }
}
