using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide
{
    internal class Controller
    {
        public ServerCore ServerCore { get; set; }
        public string LogFileName { get; set; }

        /// <summary>
        /// Constructor initialize a new instance of the Controller class with the specified IP address and port
        /// </summary>
        /// <param name="ip">The IP address to bind the server to</param>
        /// <param name="port">The port number to listen on</param>
        public Controller(string ip = Constants.DefaultIP, int port = Constants.DefaultPort) { 
            ServerCore = new ServerCore(ip, port);
            LogFileName = "";
        }

        /// <summary>
        /// Run the server by starting the server core and setting the log file name
        /// </summary>
        public void RunServer()
        {
            ServerCore.StartServer();
            LogFileName = ServerCore.LogFileName;  
        }
    }
}
