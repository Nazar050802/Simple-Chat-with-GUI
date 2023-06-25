using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide
{
    internal class Core
    {
        public Core() { }

        public void StartServer()
        {
            // Create log file
            SimpleLogs.CreateLogFile();

            // Get ip address in debug mode without global ip address
            BasicInfoIpAddress ipAddress = new BasicInfoIpAddress(true);

            Socket sListener;

            try
            {
                SocketListener socketListener = new SocketListener(ipAddress);
                sListener = socketListener.GetTcpListener();

                // Start Listen
                while (true)
                {
                    try
                    {
                        CommunicationWithClient communicationWithClient = new CommunicationWithClient(sListener);
                        communicationWithClient.StartSession();
                    }
                    catch (Exception exceptionInCommunitcation)
                    {
                        SimpleLogs.WriteToFile(exceptionInCommunitcation.ToString());
                    }
                }
            }
            catch (Exception exceptionOfCreatingSocketListener)
            {
                SimpleLogs.WriteToFile(exceptionOfCreatingSocketListener.ToString());
            }

            
        }
    }
}
