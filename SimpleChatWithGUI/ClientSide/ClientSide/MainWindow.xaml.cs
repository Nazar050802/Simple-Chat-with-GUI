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
using ClientSide;

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

        /// <summary>
        /// Create a new instance of the MainWindow class
        /// </summary>
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

        /// <summary>
        /// Attempt to connect to the server in regular intervals
        /// </summary>
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

        /// <summary>
        /// Refresh the room names at regular timeframes
        /// </summary>
        private void UpdateRoomNames()
        {
            getRoomNamesTimer.Interval = TimeSpan.FromSeconds(1);
            getRoomNamesTimer.Tick += UpdateRoomNames_Tick;
        }
        private void UpdateRoomNames_Tick(object sender, EventArgs e)
        {
            _ = controller.UpdateRoomsNamesAsync();
            roomsListBox.ItemsSource = controller.RoomsNames;
        }

        /// <summary>
        /// Display messages at regular intervals
        /// </summary>
        private void ShowMessages()
        {
            messageTimer.Interval = TimeSpan.FromSeconds(1);
            messageTimer.Tick += MessageTimer_Tick;
            messageTimer.Start();
        }

        private void MessageTimer_Tick(object sender, EventArgs e)
        {
            _ = controller.SetNewMessageAsync();

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

        /// <summary>
        /// Add a message to the chat container.
        /// </summary>
        /// <param name="userNickname">The nickname of user who sent message</param>
        /// <param name="message">The message content</param>
        /// <param name="senderOrReceiver">A flag indicating whether user is sender or the receiver of message</param>
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

        /// <summary>
        /// Generate a pastel color based on the input string
        /// </summary>
        /// <param name="inputString">Input string used to generate  color</param>
        /// <returns>Color object representing  generated pastel color</returns>
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

        /// <summary>
        /// Compute the Jenkins hash value of the input string
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>The computed Jenkins hash value</returns>
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

        /// <summary>
        /// Execute when the TextBox control gains focus. Clear default text and adjusts the opacity
        /// </summary>
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

        /// <summary>
        /// Handle the event when a selection is made in the rooms list box. Set the text of textBoxRoomNameDirectly to the selected room name
        /// </summary>
        private void RoomsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedItemName = roomsListBox.SelectedItem.ToString();

            textBoxRoomNameDirectly.Focus();

            textBoxRoomNameDirectly.Text = selectedItemName;
        }

        /// <summary>
        /// Send a message asynchrony 
        /// </summary>
        /// <param name="sender">The object that raised the event</param>
        /// <param name="e">The event arguments</param>
        private async void SendMessageAsync(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(messageTextBox.Text))
            {
                await controller.SendMessageAsync($"{messageTextBox.Text}");
                messageTextBox.Text = "";
            }
        }

        /// <summary>
        /// Handle the click event of a button asynchrony 
        /// </summary>
        /// <param name="sender">The object that raised the event</param>
        /// <param name="e">The event arguments</param>
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

        /// <summary>
        /// Handle the click event of a button to continue the process
        /// </summary>
        /// <param name="sender">The object that raised the event</param>
        /// <param name="e">The event arguments</param>
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


        /// <summary>
        /// Handle the click event of the Join button asynchrony 
        /// </summary>
        /// <param name="sender">The object that raised the event</param>
        /// <param name="e">The event arguments</param>
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

        /// <summary>
        /// Handle the click event of the Join Choosing button
        /// </summary>
        /// <param name="sender">The object that raised the event</param>
        /// <param name="e">The event arguments</param>
        private void JoinChoosingButton_Click(object sender, RoutedEventArgs e)
        {
            selectOrCreateRoomGridSelectGrid.Visibility = Visibility.Hidden;

            selectOrCreateRoomGridRoomNameGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handle the click event of the Create Choosing button
        /// </summary>
        /// <param name="sender">The object that raised the event</param>
        /// <param name="e">The event arguments</param>
        private void CreateChoosingButton_Click(object sender, RoutedEventArgs e)
        {
            selectOrCreateRoomGridSelectGrid.Visibility = Visibility.Hidden;

            selectOrCreateRoomGridCreateGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handle the click event of the Create button asynchrony 
        /// </summary>
        /// <param name="sender">The object that raised the event</param>
        /// <param name="e">The event arguments</param>
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

        /// <summary>
        /// Manage the visibility of different grids based on chat grid's visibility state
        /// </summary>
        private void MyGrid_VisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (chatGrid.Visibility == Visibility.Visible)
            {
                controller.StartReceiveMessagesFromUsers();
                textBlockRoomName.Text = controller.CurrentRoomName;
                getRoomNamesTimer.Stop();
            }
        }

        /// <summary>
        /// Manage the ButtonBackToChoose control's Click event
        /// </summary>
        public void ButtonBackToChoose_Click(object sender, RoutedEventArgs e)
        {
            selectOrCreateRoomGridRoomNameGrid.Visibility = Visibility.Hidden;
            selectOrCreateRoomGridCreateGrid.Visibility = Visibility.Hidden;

            selectOrCreateRoomGridSelectGrid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Execute when the Window is closed. Close the communication through the controller
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            controller.CloseCommunication();
        }
    }
}
