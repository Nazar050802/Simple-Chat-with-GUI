using ServerSide;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientSide
{
    internal class ClientCore
    {
        public ClientCore() { }

        public async Task StartCommunicateWithServer()
        {
            // Create log file
            SimpleLogs.CreateLogFile();

            // Get ip address in debug mode without global ip address
            BasicInfoIpAddress ipAddress = new BasicInfoIpAddress(true);

            BasicClient basicClient = new BasicClient(ipAddress);
            TcpClient client = basicClient.GetTcpClient();

            try
            {
                // Start communicate
                CommunicationWithServer communicationWithServer = new CommunicationWithServer(client, basicClient);

                await communicationWithServer.EstablishConnectionWithServer();
                communicationWithServer.StartToReveiveMessages();
                await communicationWithServer.StartToSendMessages();

                communicationWithServer.CloseConnection();

            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[ClientCore.cs][ERROR] " + ex.ToString());
            }
        }
    }
}