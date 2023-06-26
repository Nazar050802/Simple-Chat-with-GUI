using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide
{
    public interface IIpAdressProvider
    {
        IPAddress GetIPAddress();
    }

    public class ExternalIpAdressProvider : IIpAdressProvider
    {
        protected string IpAddress { get; set; }

        public ExternalIpAdressProvider(string ipAddress = "127.0.0.1")
        {
            IpAddress = ipAddress;
        }

        public IPAddress GetIPAddress()
        {
            // Get global ip
            return IPAddress.Parse(IpAddress);
        }
    }

    internal class BasicInfoIpAddress
    {
        private bool DebugMode { get; set; }
        private IIpAdressProvider IpAddressString { get; set; }

        public int Port { get; private set; }

        public BasicInfoIpAddress(bool debugMode, string ipAddressString = "127.0.0.1", int port = 11000)
        {
            DebugMode = debugMode;
            Port = port;

            // Get ip depending on whether debug mode is currently enabled. If yes get localhost. If not get global ip
            IpAddressString = new ExternalIpAdressProvider(ipAddressString);
        }

        public IPEndPoint GetIPEndPoint()
        {
            return new IPEndPoint(IpAddressString.GetIPAddress(), Port);
        }

        public IPAddress GetIPAddress()
        {
            return IpAddressString.GetIPAddress();
        }
    }
}
