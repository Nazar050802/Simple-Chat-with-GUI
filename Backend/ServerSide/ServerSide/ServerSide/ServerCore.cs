using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide
{
    internal class ServerCore
    {
        public ServerCore() { }

        public async void StartServer()
        {
            // Create log file
            SimpleLogs.CreateLogFile();

            // Get ip address in debug mode without global ip address
            BasicInfoIpAddress ipAddress = new BasicInfoIpAddress(Constants.DebugMode);
            
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
                CommunicationWithClient communicationWithClient = new CommunicationWithClient(listener);
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
