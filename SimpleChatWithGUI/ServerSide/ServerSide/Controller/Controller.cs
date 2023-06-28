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

        public Controller(string ip = Constants.DefaultIP, int port = Constants.DefaultPort) { 
            ServerCore = new ServerCore(ip, port);
            LogFileName = "";
        }

        public void RunServer()
        {
            ServerCore.StartServer();
            LogFileName = ServerCore.LogFileName;  
        }
    }
}
