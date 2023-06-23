using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide
{
    internal static class SimpleLogs
    {
        private static string fileName = $"log_{DateTime.Now.ToString("dd_MM_yyyy")}.txt";
        private static string pathToFile = Path.Combine(Environment.CurrentDirectory, fileName);

        public static bool CreateLogFile()
        {
            try
            {
                // Create log file if not exist
                if (!File.Exists(pathToFile))
                {
                    FileStream fs = File.Create(pathToFile);
                    fs.Close();

                    WriteToFile("Log file was created");
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool WriteToFile(string textToWrite)
        {
            try
            {
                using (StreamWriter wr = File.AppendText(pathToFile))
                {
                    // Add text to file
                    wr.WriteLine($"[{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}] {textToWrite}");
                    wr.Close();
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

    }
}
