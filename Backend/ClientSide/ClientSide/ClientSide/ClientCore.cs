using ServerSide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientSide
{
    internal class ClientCore
    {
        public ClientCore() { }

        public void StartCommunicateWithServer()
        {
            // Create log file
            SimpleLogs.CreateLogFile();

            // Get ip address in debug mode without global ip address
            BasicInfoIpAddress ipAddress = new BasicInfoIpAddress(true);

            Socket sender;

            try
            {
                SocketSender socketSender = new SocketSender(ipAddress);
                sender = socketSender.GetTcpSender();

                // Start communicate
                CommunicationWithServer communicationWithServer = new CommunicationWithServer(sender, ipAddress.GetIPEndPoint());

                // Establish a connection with server
                communicationWithServer.EstablishConnectionWithServer();

                Console.WriteLine("Print information for server");
                // Send message
                communicationWithServer.SendMessage(Console.ReadLine());

                // Print Receive message
                Console.WriteLine(communicationWithServer.ReceiveMessage());

                // Close connection 
                communicationWithServer.CloseConnection();
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[ClientCore.cs][ERROR] " + ex.ToString());
            }

        }
    }
}
