using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide
{
    public interface IpAdressProvider
    {
        IPAddress GetIPAddress();
    }

    public class LocalHostIpAdressProvider : IpAdressProvider
    {
        public IPAddress GetIPAddress()
        {
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            return ipHost.AddressList[0];
        }
    }

    public class ExternalIpAdressProvider : IpAdressProvider
    {
        protected string IpAddress { get; set; }

        public ExternalIpAdressProvider(string ipAddress = "127.0.0.1")
        {
            IpAddress = ipAddress;
        }

        public IPAddress GetIPAddress() 
        { 
            return IPAddress.Parse(IpAddress);
        }
    }

    internal class IpAddress
    {
        private bool DebugMode { get; set; }
        private IpAdressProvider IpAddressString { get; set;}

        private int Port { get; set; }

        public IpAddress(bool debugMode, string ipAddressString = "127.0.0.1", int port = 11000)
        {
            DebugMode = debugMode;
            Port = port;

            IpAddressString = debugMode ? (IpAdressProvider) new LocalHostIpAdressProvider() : new ExternalIpAdressProvider(ipAddressString);
        }

        public IPEndPoint GetIPEndPoint()
        {
            return new IPEndPoint(IpAddressString.GetIPAddress(), Port);
        }
    }
}
