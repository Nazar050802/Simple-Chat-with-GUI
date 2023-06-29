using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ClientSide
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string TextBoxPreviewText { get; set; } = "Type here...";
        private List<(string userName, string message, bool senderOrReceiver)> chatMessages;
        private DispatcherTimer messageTimer;

        public MainWindow()
        {
            InitializeComponent();

            chatMessages = new List<(string userName, string message, bool senderOrReceiver)>();
            messageTimer = new DispatcherTimer();

            ShowMessages();

            DataContext = this;
        }

        private void ShowMessages()
        {
            messageTimer.Interval = TimeSpan.FromSeconds(1); 
            messageTimer.Tick += MessageTimer_Tick;
            messageTimer.Start();
        }

        private void MessageTimer_Tick(object sender, EventArgs e)
        {
            // Check for new messages in the Messages dictionary
            foreach (var message in chatMessages)
            {
                string userNickname = message.userName;
                string messageContent = message.message;
                bool senderOrReceiver = message.senderOrReceiver;
                AddMessageToChat(userNickname, messageContent, senderOrReceiver);
            }

            chatMessages.Clear();
        }

        private void AddMessageToChat(string userNickname, string message, bool senderOrReceiver)
        {
            double maxMessageWidth = (this.ActualWidth / 2) * 1.25;

            // Create UI elements
            Border messageContainer = new Border
            {
                HorizontalAlignment = senderOrReceiver ? HorizontalAlignment.Left : HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(11.5),
                Background = new SolidColorBrush(senderOrReceiver ? Color.FromRgb(26, 40, 55) : Color.FromRgb(43, 76, 120)),
                Width = Double.NaN,
                MaxWidth = maxMessageWidth
            };

            Grid messageGrid = new Grid
            {
                Margin = new Thickness(10)
            };

            Label nickname = new Label
            {
                Content = userNickname,
                // Generate a unique color for user nickname
                Foreground = new SolidColorBrush(GeneratePastelColorFromString(userNickname)),
                FontWeight = FontWeights.Bold
            };

            TextBlock messageTextBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.White
            };

            // Set up the message container structure
            messageGrid.RowDefinitions.Add(new RowDefinition());
            messageGrid.RowDefinitions.Add(new RowDefinition());

            Grid.SetRow(nickname, 0);
            Grid.SetRow(messageTextBlock, 1);

            messageGrid.Children.Add(nickname);
            messageGrid.Children.Add(messageTextBlock);

            // Add the UI elements to the message container
            messageContainer.Child = messageGrid;

            messageContainer.Margin = new Thickness(10, 0, 10, 10);

            // Add the message container to the chat container
            ChatContainer.Children.Add(messageContainer);
            ChatScrollViewer.ScrollToBottom();
        }

        private void SendMessage(object sender, RoutedEventArgs e)
        {
            chatMessages.Add(("User1111", MessageTextBox.Text, true));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            chatMessages.Add(("User2", MessageTextBox.Text, false));
        }

        private Color GeneratePastelColorFromString(string inputString)
        {
            // Get hash from string
            uint hash = JenkinsHash(inputString);

            byte r = (byte)((hash & 0xFF0000) >> 16);
            byte g = (byte)((hash & 0x00FF00) >> 8);
            byte b = (byte)(hash & 0x0000FF);

            r = (byte)((r + 255) / 2);
            g = (byte)((g + 255) / 2);
            b = (byte)((b + 255) / 2);

            return Color.FromRgb(r, g, b);
        }

        private uint JenkinsHash(string input)
        {
            uint hash = 0;

            foreach (char c in input)
            {
                hash += c;
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }

            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);

            return hash;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            chatGrid.Visibility = Visibility.Visible; 
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (tb.Text == TextBoxPreviewText)
            {
                tb.Text = string.Empty;
            }

            tb.Opacity = 1;
        }

    }
}
