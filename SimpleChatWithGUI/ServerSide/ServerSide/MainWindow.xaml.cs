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
        Controller controller;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void CreateServer(string ip, int port)
        {
            controller = new Controller(ip, port);
            controller.RunServer();

            ShowLogSystem();
        }

        public void ShowLogSystem()
        {
            // Start a timer to refresh the log every 1 second
            Timer refreshTimer = new Timer(1000);
            refreshTimer.Elapsed += RefreshTimer_Elapsed;
            refreshTimer.Start();

            // Initial read and display of the log file
            ReadAndDisplayLog();
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Check if the log file has been modified
            if (File.GetLastWriteTime(controller.LogFileName) > lastModifiedTime)
            {
                // If the file has been modified, read and display the log
                ReadAndDisplayLog();
            }
        }

        private DateTime lastModifiedTime;

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
            catch (Exception ex) {
                
            }
        }

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
