using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ClientSide
{
    public interface IIpAdressProvider
    {
        /// <summary>
        /// Gets the IP address
        /// </summary>
        /// <returns>The IP address as an instance of the IPAddress class</returns>
        IPAddress GetIPAddress();
    }

    public class ExternalIpAdressProvider : IIpAdressProvider
    {
        protected string IpAddress { get; set; }

        /// <summary>
        /// Constructor that initializes the IpAddress property with the provided ipAddress parameter or the default IP address from the Constants
        /// </summary>
        /// <param name="ipAddress">The IP address as a string. Default is Constants.DefaultIP</param>
        public ExternalIpAdressProvider(string ipAddress = Constants.DefaultIP)
        {
            IpAddress = ipAddress;
        }

        /// <summary>
        /// Parses the stored IP address string and returns it as an instance of the IPAddress class
        /// </summary>
        /// <returns>The IP address as an instance of the IPAddress class</returns>
        public IPAddress GetIPAddress()
        {
            
            return IPAddress.Parse(IpAddress);
        }
    }

    public class LocalIpAdressProvider : IIpAdressProvider
    {
        protected string IpAddress { get; set; }

        /// <summary>
        /// Constructor that initializes the IpAddress property with the provided ipAddress parameter or the default IP address from the Constants
        /// </summary>
        /// <param name="ipAddress">The IP address as a string. Default is Constants.DefaultIP.</param>
        public LocalIpAdressProvider(string ipAddress = Constants.DefaultIP)
        {
            IpAddress = ipAddress;
        }

        /// <summary>
        /// Parses the stored IP address string and returns it as an instance of the IPAddress class
        /// </summary>
        /// <returns>The IP address as an instance of the IPAddress class</returns>
        public IPAddress GetIPAddress()
        {

            return IPAddress.Parse(IpAddress);
        }
    }

    public class BasicInfoIpAddress
    {
        private bool DebugMode { get; set; }
        private IIpAdressProvider IpAddressString { get; set; }

        public virtual int Port { get; private set; }

        /// <summary>
        /// Constructor initializes the DebugMode, Port, and IpAddress properties with the provided parameters or default values from the Constants
        /// </summary>
        /// <param name="debugMode">A boolean indicating whether debug mode is enabled</param>
        /// <param name="ipAddressString">The IP address as a string. Default is Constants.DefaultIP</param>
        /// <param name="port">The port number. Default is Constants.DefaultPort</param>
        public BasicInfoIpAddress(bool debugMode, string ipAddressString = Constants.DefaultIP, int port = Constants.DefaultPort)
        {
            DebugMode = debugMode;
            Port = port;

            // Get ip depending on whether debug mode is currently enabled. If yes get localhost. If not get global ip
            IpAddressString = debugMode ? new LocalIpAdressProvider(ipAddressString) : new ExternalIpAdressProvider(ipAddressString);
        }

        /// <summary>
        /// Retrieve the IPAddress instance from the IpAddress property and creates a new IPEndPoint instance using that IP address and the stored Port value
        /// </summary>
        /// <returns>An instance of the IPEndPoint class</returns>
        public IPEndPoint GetIPEndPoint()
        {
            return new IPEndPoint(IpAddressString.GetIPAddress(), Port);
        }

        /// <summary>
        /// Retrieve the IPAddress instance from the IpAddress property and returns it
        /// </summary>
        /// <returns>The IP address as an instance of the IPAddress class</returns>
        public virtual IPAddress GetIPAddress()
        {
            return IpAddressString.GetIPAddress();
        }
    }
}
