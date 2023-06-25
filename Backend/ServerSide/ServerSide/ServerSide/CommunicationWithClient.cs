using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerSide
{
    internal class CommunicationWithClient
    {
        private Socket SListener { get; set; }

        public CommunicationWithClient(Socket sListener) 
        {
            SListener = sListener;
        }

        public void StartSession()
        {
            try 
            { 
                Socket handler = SListener.Accept();
                ThreadPool.QueueUserWorkItem(HandleClient, handler);

                int workerThreads, completionPortThreads;
                ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);

                // +1 for the current thread
                Console.WriteLine("Active Threads: " + (workerThreads + 1)); 
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile(ex.ToString());
            }
        }

        private void HandleClient(object handlerObj)
        {
            Socket handler = (Socket)handlerObj;
            var clientHandler = new ClientHandler(handler);
            clientHandler.Handle();
        }
    }

    public class ClientHandler
    {
        private Socket Handler { get; set; }

        public ClientHandler(Socket handler)
        {
            Handler = handler;
        }

        public void Handle()
        {
            try
            {
                InteractWithClient interactWithClient = new InteractWithClient(Handler);

                // Create a Timeout for Connection with Client
                Handler.ReceiveTimeout = 150000;

                // Get data from Client
                string requestData = interactWithClient.ReceiveData();
                Console.WriteLine(requestData);

                // Send reply to Client
                string reply = interactWithClient.GenerateReply(requestData);
                interactWithClient.SendData(reply);
            }
            catch (SocketException ex)
            {
                // Exception for Timeout Error
                if (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    HandleTimeout(ex);
                }
                else
                {
                    HandleException(ex, "[ERROR]");
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "[ERROR]");
            }
            finally 
            {
                CloseConnection();
            }
        }

        private void HandleTimeout(Exception ex)
        {
            string timeoutMessage = "The connection was closed due to a timeout";
            byte[] timeoutBytes = Encoding.UTF8.GetBytes(timeoutMessage);
            Handler.Send(timeoutBytes);

            Console.WriteLine(timeoutMessage);

            HandleException(ex, "[INFO]");
        }

        private void HandleException(Exception ex, string additionalText = "")
        {
            SimpleLogs.WriteToFile($"[CommunicationWithClient.cs] {additionalText} " + ex.ToString());
        }

        private bool CloseConnection()
        {
            try
            {
                Handler.Shutdown(SocketShutdown.Both);
                Handler.Close();

                Console.WriteLine("The thread has finished running.");
            }
            catch (Exception ex)
            {
                SimpleLogs.WriteToFile("[CommunicationWithClient.cs][ERROR] " + ex.ToString());
                return false;
            }

            return true;
        }

        private bool IsClientConnected()
        {
            return Handler.Connected;
        }
    }

    class InteractWithClient
    {
        private Socket Handler { get; set; }

        public InteractWithClient(Socket handler)
        {
            Handler = handler;
        }

        public string ReceiveData()
        {
            byte[] buffer = new byte[1024];
            int bytesRead = Handler.Receive(buffer);

            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        public string GenerateReply(string requestData)
        {
            string reply = "Thank you for your request to " + requestData.Length.ToString() + " symbols";
            return reply;
        }

        public void SendData(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            Handler.Send(bytes);
        }
    }
}
