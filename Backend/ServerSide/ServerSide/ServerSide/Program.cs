using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SimpleLogs.CreateLogFile();
            SimpleLogs.WriteToFile("Test");

        }
    }
}
