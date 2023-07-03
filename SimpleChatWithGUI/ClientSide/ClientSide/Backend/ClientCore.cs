using ServerSide;
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

        public ClientCore(string ip, int port)
        {
            communicationWithServer = new CommunicationWithServer();

            // Get ip address in debug mode without global ip address
            ipAddress = new BasicInfoIpAddress(Constants.DebugMode, ip, port);
        }

        public async Task StartCommunicateWithServer()
        {
            // Create log file
            SimpleLogs.CreateLogFile();

            // Get ip address in debug mode without global ip address
            BasicInfoIpAddress ipAddress = new BasicInfoIpAddress(true, Constants.DefaultIP, Constants.DefaultPort);

            BasicClient basicClient = new BasicClient(ipAddress);
            TcpClient client = basicClient.GetTcpClient();

            try
            {
                // Start communicate
                communicationWithServer = new CommunicationWithServer(client, basicClient);

                await communicationWithServer.EstablishConnectionWithServer();
                await communicationWithServer.InitialSetting();

            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[ClientCore.cs][ERROR] " + ex.ToString());
                communicationWithServer.CloseConnection();
            }
        }
    }
}