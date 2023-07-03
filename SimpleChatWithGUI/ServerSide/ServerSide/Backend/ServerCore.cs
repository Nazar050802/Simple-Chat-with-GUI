using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide
{
    public class ServerCore
    {
        public CommunicationWithClient communicationWithClient;
        public string LogFileName { get; set; }

        private BasicInfoIpAddress ipAddress;

        public ServerCore(string ip=Constants.DefaultIP, int port=Constants.DefaultPort) {
            communicationWithClient = new CommunicationWithClient();
            LogFileName = "";

            // Get ip address in debug mode without global ip address
            ipAddress = new BasicInfoIpAddress(Constants.DebugMode, ip, port);
        }

        public async void StartServer()
        {
            // Create log file
            SimpleLogs.CreateLogFile();

            // Set log fileName
            LogFileName = SimpleLogs.FileName;

            // Initialize listener
            BasicListener sListener = new BasicListener(ipAddress);
            TcpListener listener = sListener.GetTcpListener();

            try
            {
                listener.Start();
            }
            catch (Exception ex) { 
                SimpleLogs.WriteToFile("[ServerCore.cs][ERROR] " + ex.ToString());
            }

            try
            {
                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    _ = communicationWithClient.HandleClientAsync(client);
                }
            }
            catch (SocketException ex)
            {
                SimpleLogs.WriteToFile("[ServerCore.cs][ERROR] " + ex.ToString());
            }
            finally
            {
                listener.Stop();
            }
            
        }
    }
}
