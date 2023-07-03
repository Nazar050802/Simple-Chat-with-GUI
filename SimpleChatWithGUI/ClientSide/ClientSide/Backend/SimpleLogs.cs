using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSide
{
    public static class SimpleLogs
    {
        private static string FileName { get; set; } = $"log_{DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss")}.txt";
        private static string PathToFile { get; set; } = Path.Combine(Environment.CurrentDirectory, FileName);

        public static bool CreateLogFile(string pathToFile = "")
        {
            string finalPathFile = pathToFile == "" ? PathToFile : pathToFile;

            try
            {
                // Create log file if not exist
                if (!File.Exists(finalPathFile))
                {
                    FileStream fs = File.Create(finalPathFile);
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

        public static bool WriteToFile(string textToWrite, string pathToFile = "")
        {
            try
            {
                using (StreamWriter wr = File.AppendText(pathToFile == "" ? PathToFile : pathToFile))
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
