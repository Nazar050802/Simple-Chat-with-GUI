using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using ServerSide;

namespace ClientSide
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string TextBoxPreviewText { get; set; } = "Type here...";


        private DispatcherTimer messageTimer = new DispatcherTimer();
        private DispatcherTimer connectionTimer = new DispatcherTimer();
        private DispatcherTimer getRoomNamesTimer = new DispatcherTimer();

        Controller controller;

        public MainWindow()
        {
            InitializeComponent();

            controller = new Controller();
            controller.StartCommunicate();

            ShowMessages();
            TryToConnectToTheServer();
            UpdateRoomNames();

            DataContext = this;
            roomsListBox.DataContext = controller;

        }

        private void TryToConnectToTheServer()
        {
            connectionTimer.Interval = TimeSpan.FromSeconds(1);
            connectionTimer.Tick += TryToConnectToTheServer_Tick;
            connectionTimer.Start();
        }
        private void TryToConnectToTheServer_Tick(object sender, EventArgs e)
        {
            if (controller.IsClientConnetToTheServer())
            {
                clientConnectToServerGrid.Visibility = Visibility.Hidden;
                userDataGrid.Visibility = Visibility.Visible;
                connectionTimer.Stop();
                textBoxUsername.Text = "";
            }
        }

        private void UpdateRoomNames()
        {
            getRoomNamesTimer.Interval = TimeSpan.FromSeconds(1);
            getRoomNamesTimer.Tick += UpdateRoomNames_Tick;
        }
        private void UpdateRoomNames_Tick(object sender, EventArgs e)
        {
            _ = controller.UpdateRoomsNames();
            roomsListBox.ItemsSource = controller.RoomsNames;
        }

        private void ShowMessages()
        {
            messageTimer.Interval = TimeSpan.FromSeconds(1);
            messageTimer.Tick += MessageTimer_Tick;
            messageTimer.Start();
        }

        private void MessageTimer_Tick(object sender, EventArgs e)
        {
            _ = controller.SetNewMessage();

            // Check for new messages in the Messages dictionary
            foreach (var message in controller.GetNewMessages())
            {
                string userNickname = message.Username;
                string messageContent = message.Text;
                bool senderOrReceiver = message.SenderOrReceiver;
                AddMessageToChat(userNickname, messageContent, senderOrReceiver);
            }

            controller.VariableUpdateChatMessages();
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

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Change opacity of text in textBox
            TextBox tb = (TextBox)sender;

            if (tb.Text == TextBoxPreviewText)
            {
                tb.Text = string.Empty;
            }

            tb.Opacity = 1;
        }

        private void RoomsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedItemName = roomsListBox.SelectedItem.ToString();

            textBoxRoomNameDirectly.Focus();

            textBoxRoomNameDirectly.Text = selectedItemName;
        }

        private async void SendMessageAsync(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(messageTextBox.Text))
            {
                await controller.SendMessage($"{messageTextBox.Text}");
                messageTextBox.Text = "";
            }
        }

        private async void Button_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxUsername.Text))
            {
                string username = textBoxUsername.Text;
                if (await controller.TryToSetUsername(username))
                {
                    selectOrCreateRoomGrid.Visibility = Visibility.Visible;
                    selectOrCreateRoomGridSelectGrid.Visibility = Visibility.Visible;
                    userDataGrid.Visibility = Visibility.Hidden;

                    getRoomNamesTimer.Start();
                }
                else
                {
                    labelUsernameAlreadyUse.Visibility = Visibility.Visible;
                }
            }
        }
        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxRoomNameDirectly.Text))
            {
                if (controller.RoomsNames.Contains(textBoxRoomNameDirectly.Text))
                {
                    selectOrCreateRoomGridRoomNameGrid.Visibility = Visibility.Hidden;
                    selectOrCreateRoomGridPasswordGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    inputCorrectRoomName.Visibility = Visibility.Visible;
                }
            }
        }

        private async void JoinButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxRoomNameDirectly.Text) && !string.IsNullOrWhiteSpace(textBoxRoomPassword.Text))
            {
                string roomName = textBoxRoomNameDirectly.Text;
                string password = textBoxRoomPassword.Text;
                if (await controller.TryToJoinRoom(roomName, password))
                {
                    chatGrid.Visibility = Visibility.Visible;
                    selectOrCreateRoomGridPasswordGrid.Visibility = Visibility.Hidden;
                }
                else
                {
                    labelRoomPasswordIncorrect.Visibility = Visibility.Visible;
                }
            }
        }

        private void JoinChoosingButton_Click(object sender, RoutedEventArgs e)
        {
            selectOrCreateRoomGridSelectGrid.Visibility = Visibility.Hidden;

            selectOrCreateRoomGridRoomNameGrid.Visibility = Visibility.Visible;
        }

        private void CreateChoosingButton_Click(object sender, RoutedEventArgs e)
        {
            selectOrCreateRoomGridSelectGrid.Visibility = Visibility.Hidden;

            selectOrCreateRoomGridCreateGrid.Visibility = Visibility.Visible;
        }
        private async void CreateButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxCreateRoomName.Text) && !string.IsNullOrWhiteSpace(textBoxCreateRoomPassword.Text))
            {
                string roomName = textBoxCreateRoomName.Text;
                string password = textBoxCreateRoomPassword.Text;
                if (await controller.TryToCreateRoom(roomName, password))
                {
                    chatGrid.Visibility = Visibility.Visible;
                    selectOrCreateRoomGridCreateGrid.Visibility = Visibility.Hidden;
                }
                else
                {
                    labelUsernameAlreadyUse.Visibility = Visibility.Visible;
                }
            }
        }

        private void MyGrid_VisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (chatGrid.Visibility == Visibility.Visible)
            {
                controller.StartReceiveMessagesFromUsers();
                textBlockRoomName.Text = controller.CurrentRoomName;
                getRoomNamesTimer.Stop();
            }
        }

        public void ButtonBackToChoose_Click(object sender, RoutedEventArgs e)
        {
            selectOrCreateRoomGridRoomNameGrid.Visibility = Visibility.Hidden;
            selectOrCreateRoomGridCreateGrid.Visibility = Visibility.Hidden;

            selectOrCreateRoomGridSelectGrid.Visibility = Visibility.Visible;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            controller.CloseCommunication();
        }
    }
}
