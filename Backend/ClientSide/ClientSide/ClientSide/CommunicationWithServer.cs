using ServerSide;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientSide
{
    internal class CommunicationWithServer
    {
        private Socket Sender { get; set; }
        private IPEndPoint IpEndPoint {get; set;}

        public CommunicationWithServer(Socket sender, IPEndPoint ipEndPoint)
        {
            IpEndPoint = ipEndPoint;    
            Sender = sender;
        }
        
        public void EstablishConnectionWithServer()
        {
            while (!IsClientConnected())
            {
                try
                {
                    Sender.Connect(IpEndPoint);
                }
                catch (Exception ex)
                {
                    SimpleLogs.WriteToFile("[CommunicationWithServer.cs][ERROR] " + ex.ToString());
                    Thread.Sleep(1000);
                }
            }
        }

        public bool SendStringMessage(string message="")
        {
            try
            {
                InteractWithServer interactionWithServer = new InteractWithServer(Sender);
                interactionWithServer.SendString(message);
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[CommunicationWithServer.cs][ERROR] " + ex.ToString());
                return false;
            }

            return true;
        }

        public bool SendRawData(byte[] bytes)
        {
            try
            {
                InteractWithServer interactionWithServer = new InteractWithServer(Sender);
                interactionWithServer.SendRawData(bytes);
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[CommunicationWithServer.cs][ERROR] " + ex.ToString());
                return false;
            }

            return true;
        }

        public string ReceiveStringMessage()
        {
            try
            {
                InteractWithServer interactionWithServer = new InteractWithServer(Sender);
                return interactionWithServer.ReceiveString();
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[CommunicationWithServer.cs][ERROR] " + ex.ToString());
                return "";
            }
        }

        public byte[] ReceiveRawData()
        {
            try
            {
                InteractWithServer interactionWithServer = new InteractWithServer(Sender);
                return interactionWithServer.ReceiveRawData();
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[CommunicationWithServer.cs][ERROR] " + ex.ToString());
                return new byte[] { };
            }
        }

        public bool CloseConnection()
        {
            try
            {
                Sender.Shutdown(SocketShutdown.Both);
                Sender.Close();

                Console.WriteLine("The communication has finished.");
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[CommunicationWithServer.cs][ERROR] " + ex.ToString());
                return false;
            }

            return true; 
        }

        private bool IsClientConnected()
        {
            return Sender.Connected;
        }

    }

    class InteractWithServer
    {
        private Socket Sender { get; set; }

        public InteractWithServer(Socket handler)
        {
            Sender = handler;
        }

        public string ReceiveString()
        {
            byte[] buffer = new byte[Constants.BufferSize];
            int bytesRead = Sender.Receive(buffer);

            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        public byte[] ReceiveRawData()
        {
            byte[] buffer = new byte[Constants.BufferSize];
            Sender.Receive(buffer);

            return buffer;
        }

        public void SendString(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            Sender.Send(bytes);
        }

        public void SendRawData(byte[] bytes)
        {
            Sender.Send(bytes);
        }
      
    }
}
