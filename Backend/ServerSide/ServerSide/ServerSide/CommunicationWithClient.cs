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
        private Socket handler;

        public ClientHandler(Socket handler)
        {
            this.handler = handler;
        }

        public void Handle()
        {
            try
            {
                handler.ReceiveTimeout = 5000;

                string requestData = ReceiveData();
                Console.WriteLine(requestData);

                string reply = GenerateReply(requestData);
                SendData(reply);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    HandleTimeout(ex);
                }
                else
                {
                    HandleException(ex);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            finally 
            {
                CloseConnection();
            }
        }

        private string ReceiveData()
        {
            byte[] buffer = new byte[1024];
            int bytesRead = handler.Receive(buffer);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        private string GenerateReply(string requestData)
        {
            string reply = "Thank you for your request to" + requestData.Length.ToString() + " symbols";
            return reply;
        }

        private void SendData(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            handler.Send(bytes);
        }

        private void HandleTimeout(Exception ex)
        {
            string timeoutMessage = "The connection was closed due to a timeout";
            byte[] timeoutBytes = Encoding.UTF8.GetBytes(timeoutMessage);
            handler.Send(timeoutBytes);

            Console.WriteLine(timeoutMessage);

            HandleException(ex);
        }

        private void HandleException(Exception ex)
        {
            SimpleLogs.WriteToFile("[CommunicationWithClient.cs]" + ex.ToString());
        }

        private void CloseConnection()
        {
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();

            Console.WriteLine("The thread has finished running.");
        }
    }
}
