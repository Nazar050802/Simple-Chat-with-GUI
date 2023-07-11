using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Net;

namespace ServerSide
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Controller controller {get; set;}

        private DateTime lastModifiedTime { get; set; }

        /// <summary>
        /// Constructor initialize a new instance of the MainWindow class
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Create and starts the server at the given IP address and port
        /// </summary>
        /// <param name="ip">The IP address for the server</param>
        /// <param name="port">The port for the server</param>
        public void CreateServer(string ip, int port)
        {
            controller = new Controller(ip, port);
            controller.RunServer();

            ShowLogSystem();
        }

        /// <summary>
        /// Displays the log system by starting a timer to update the log every 1 second and showing the initial log contents
        /// </summary>
        public void ShowLogSystem()
        {
            // Start a timer to refresh the log every 1 second
            Timer refreshTimer = new Timer(1000);
            refreshTimer.Elapsed += RefreshTimer_Elapsed;
            refreshTimer.Start();

            // Initial read and display of the log file
            ReadAndDisplayLog();
        }

        /// <summary>
        /// Check if the log file has been updated every time the timer elapses, and if so, reads and displays the updated log
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event data</param>
        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Check if the log file has been modified
            if (File.GetLastWriteTime(controller.LogFileName) > lastModifiedTime)
            {
                // If the file has been modified, read and display the log
                ReadAndDisplayLog();
            }
        }

        /// <summary>
        /// Reads the contents of the log file and displays them in the log text box
        /// </summary>
        private void ReadAndDisplayLog()
        {
            try
            {
                // Read the log file contents
                string logText = File.ReadAllText(controller.LogFileName);

                // Update the textbox text on the UI thread
                Dispatcher.Invoke(() =>
                {
                    logTextBox.Text = logText;
                    logTextBox.ScrollToEnd();
                });

                // Update the last modified time
                lastModifiedTime = File.GetLastWriteTime(controller.LogFileName);
            }
            catch (Exception ex) {}
        }

        /// <summary>
        /// Handle the event when the 'Start Server' button is clicked. It gets the IP address and port from the text boxes,
        /// validates them, and starts the server at the specified IP address and port
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event data</param>
        private void Button_Click_Start_Server(object sender, RoutedEventArgs e)
        {
            // Get the IP address and port from the text boxes
            string ipAddress = ipTextBox.Text;
            string portString = portTextBox.Text;

            // Validate and parse the IP address
            IPAddress ip;
            if (!IPAddress.TryParse(ipAddress, out ip))
            {
                MessageBox.Show("Invalid IP address format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Validate and parse the port number
            int port;
            if (!int.TryParse(portString, out port) || port < 0 || port > 65535)
            {
                MessageBox.Show("Invalid port number. Please enter a value between 0 and 65535.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            startServer.IsEnabled = false;
            ipTextBox.IsEnabled = false;
            portTextBox.IsEnabled = false;  

            CreateServer(ipAddress, port);
        }
    }
}
