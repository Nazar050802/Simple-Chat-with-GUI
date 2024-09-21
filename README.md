# Simple Chat with GUI

![License](https://img.shields.io/badge/License-MIT-yellow.svg)

## Table of Contents
- [Overview](#overview)
- [Features](#features)
- [Technologies Used](#technologies-used)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Running the Application](#running-the-application)
- [Usage](#usage)
  - [Server-Side](#server-side)
  - [Client-Side](#client-side)
- [Security](#security)
- [Testing](#testing)
- [Future Enhancements](#future-enhancements)
- [Contributing](#contributing)
- [License](#license)
- [Acknowledgments](#acknowledgments)

## Overview

**Simple Chat with GUI** is a user-friendly chat application designed to facilitate real-time communication between users in a secure, chatroom-like environment. Built with a graphical user interface (GUI) using Windows Presentation Foundation (WPF), the application comprises both server-side and client-side components, enabling seamless interaction and message exchange.

![Chat Application Screenshot](https://github.com/Nazar050802/Simple-Chat-with-GUI/blob/main/documentation/pictures/chat_screenshot.png)

## Features

- **Graphical User Interface:** Intuitive and modern UI built with WPF and the ModernWPF UI Library.
- **Real-Time Communication:** Send and receive text messages instantly using the TCP protocol.
- **Secure Messaging:** Implements RSA encryption to ensure secure message exchange.
- **Chatrooms:** Create and join password-protected chatrooms for private conversations.
- **User Management:** Unique username enforcement to prevent duplication.
- **Logging:** Comprehensive logging of server activities for monitoring and debugging.
- **Extensible Architecture:** Modular codebase allowing for easy enhancements and feature additions.

## Technologies Used

- **Programming Language:** C#
- **Framework:** .NET Framework
- **GUI Framework:** Windows Presentation Foundation (WPF)
- **Encryption:** RSA Cryptosystem
- **Version Control:** GitHub

## Getting Started

Follow these instructions to set up and run the Simple Chat application on your local machine.

### Prerequisites

- **Operating System:** Windows 10 or later
- **.NET Framework:** Version 4.7.2 or higher
- **Visual Studio:** Recommended for building from source (optional)

### Installation

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/Nazar050802/Simple-Chat-with-GUI.git
   ```
2. **Navigate to the Project Directory:**
   ```bash
   cd Simple-Chat-with-GUI
   ```

### Running the Application

The application consists of two main components: the server and the client. Both need to be run separately.

1. **Start the Server:**
   - Navigate to the server executable:
     ```
     Simple-Chat-with-GUI/Backend/ServerSide/ServerSide/ServerSide/bin/Debug/ServerSide.exe
     ```
   - Double-click `ServerSide.exe` to launch the server. The server will start listening for client connections and log activities.

2. **Start the Client:**
   - Navigate to the client executable:
     ```
     Simple-Chat-with-GUI/Backend/ClientSide/ClientSide/ClientSide/bin/Debug/ClientSide.exe
     ```
   - Double-click `ClientSide.exe` to launch the client application. You can then connect to the server using the provided interface.

## Usage

### Server-Side

- **Configuration:** Before starting, configure the server settings such as IP address and port if necessary.
- **Logging:** The server logs all activities, including client connections, message exchanges, and errors. Logs are stored with timestamped filenames for easy reference.
- **Managing Chatrooms:** The server handles the creation, deletion, and management of chatrooms based on user interactions.

### Client-Side

1. **Connecting to the Server:**
   - Upon launching, the client enters a waiting state until it successfully connects to the server.
   - Enter a unique username when prompted. If the username is already taken, you will be asked to choose another one.

2. **Chatroom Selection:**
   - **Create a Room:** Choose to create a new chatroom by providing a room name and a password.
   - **Join a Room:** Select an existing chatroom from the list or enter the room name manually. Provide the correct password to join.

3. **Messaging:**
   - Once inside a chatroom, you can send and receive messages in real-time.
   - Messages are encrypted using RSA before being sent to ensure privacy and security.

## Security

Security is a paramount concern in the design of the Simple Chat application. Here's how security is implemented:

- **RSA Encryption:** All messages exchanged between clients and the server are encrypted using RSA to prevent unauthorized access and ensure data integrity.
- **Unique User Identification:** Users are identified by unique IDs generated through SHA-1 hashing of their connection details, enhancing security and anonymity.
- **Password-Protected Chatrooms:** Each chatroom requires a password, ensuring that only authorized users can access private conversations.
- **Secure Code Verification:** The server issues a security code to each client upon connection. This code must be included in subsequent requests, preventing unauthorized actions.
- **No Data Persistence:** The server does not store any user data or conversation history. All messages are transient and deleted once all participants leave the chatroom, ensuring user privacy.

![Security Architecture](https://github.com/Nazar050802/Simple-Chat-with-GUI/blob/main/documentation/pictures/RSA.jpg)

## Testing

To ensure the reliability and correctness of the application, a series of unit and integration tests have been implemented:

- **Unit Tests:** Validate individual components and methods within the server and client applications.
- **Integration Tests:** Verify the interaction between the server and client, ensuring seamless communication and functionality.
  
**Note:** Some integration tests require a running server or client. Refer to the code comments for activation instructions.

## Future Enhancements

The following features are planned for future development to enhance the functionality and user experience of the Simple Chat application:

- **Improved User Interface:** Enhancements to the GUI for a more engaging and user-friendly experience.
- **File Transfer:** Ability to send and receive files within chatrooms.
- **Conversation Persistence:** Option for users to save their chat history in a secure database for future reference.
- **Enhanced Security Measures:** Additional security protocols to further safeguard user data and communications.
- **Mobile Support:** Extending the application to support mobile platforms for broader accessibility.

## Contributing

Contributions are welcome! This project is currently maintained for educational purposes. To contribute:

1. **Fork the Repository**
2. **Create a Feature Branch**
   ```bash
   git checkout -b feature/AmazingFeature
   ```
3. **Commit Your Changes**
   ```bash
   git commit -m 'Add some AmazingFeature'
   ```
4. **Push to the Branch**
   ```bash
   git push origin feature/AmazingFeature
   ```
5. **Open a Pull Request**

Please ensure that your contributions adhere to the project's coding standards and include appropriate tests.

## License

This project is licensed under the [MIT License](LICENSE). See the [LICENSE](LICENSE) file for details.

## Acknowledgments

- **Charles University Faculty of Mathematics and Physics:** For providing the academic environment and resources.
- **Programming 2 [NPRG031] Course:** For the foundational knowledge and support during the development of this project.
- **ModernWPF UI Library:** For enabling the creation of a modern and responsive user interface.
- **Open Source Community:** For the invaluable resources and contributions that made this project possible.

---

*Developed by Nazar Mozharov, Prague 2023.*
