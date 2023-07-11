using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide
{
    public static class SimpleLogs
    {
        public static string FileName { get; set; } = $"log_{DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss")}.txt";
        private static string PathToFile { get; set; } = Path.Combine(Environment.CurrentDirectory, FileName);

        /// <summary>
        /// Create a log file at a given location, or at a default location if none is provided
        /// </summary>
        /// <param name="pathToFile">The desired location for the log file (optional)</param>
        /// <returns>Returns true if the log file was successfully generated, false otherwise</returns>
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

        /// <summary>
        /// Write a given text into the log file located at a specific or default path, as provided
        /// </summary>
        /// <param name="textToWrite">The content to be added into the log file</param>
        /// <param name="pathToFile">The location of the log file (optional)</param>
        /// <returns>Returns true if the text was successfully added to the log file, false otherwise</returns>
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
