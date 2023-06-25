using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ClientSide
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ClientCore core = new ClientCore();
            core.StartCommunicateWithServer();

            Console.ReadLine();
        }
    }
}
