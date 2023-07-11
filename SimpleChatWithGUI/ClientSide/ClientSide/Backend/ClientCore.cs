
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientSide
{
    public class ClientCore
    {
        public CommunicationWithServer communicationWithServer;

        private BasicInfoIpAddress ipAddress;

        /// <summary>
        /// Constructor initializes a new instance of the ClientCore class and sets the IP address and port for communication
        /// </summary>
        /// <param name="ip">The IP address as a string</param>
        /// <param name="port">The port number</param>
        public ClientCore(string ip, int port)
        {
            communicationWithServer = new CommunicationWithServer();

            // Get ip address in debug mode without global ip address
            ipAddress = new BasicInfoIpAddress(Constants.DebugMode, ip, port);
        }


        /// <summary>
        /// Start the communication with the server asynchrony 
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task StartCommunicateWithServerAsync()
        {
            // Create log file
            SimpleLogs.CreateLogFile();

            // Get ip address in debug mode without global ip address
            BasicInfoIpAddress ipAddress = new BasicInfoIpAddress(true, Constants.DefaultIP, Constants.DefaultPort);

            BasicClient basicClient = new BasicClient(ipAddress);
            TcpClient client = basicClient.GetTcpClient();

            try
            {
                // Start the communication with the server
                communicationWithServer = new CommunicationWithServer(client, basicClient);

                await communicationWithServer.EstablishConnectionWithServer();
                await communicationWithServer.InitialSettingAsync();

            }
            catch (Exception ex)
            {
                // Write an error message to the log file
                SimpleLogs.WriteToFile("[ClientCore.cs][ERROR] " + ex.ToString());
                communicationWithServer.CloseConnection();
            }
        }
    }
}