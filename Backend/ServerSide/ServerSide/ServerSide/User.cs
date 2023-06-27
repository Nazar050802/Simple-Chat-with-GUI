using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace ServerSide
{
    internal class User
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public RSAGenerating rsaGeneratingServer  {get; set; }

        public RSAGenerating rsaGeneratingClient { get; set; }

        public TcpClient TcpConnection { get; set; }

        public int CurrentRoomId { get; set; }

        public User(TcpClient client) {

            TcpConnection = client;
            rsaGeneratingClient = new RSAGenerating();
            rsaGeneratingServer = new RSAGenerating();

            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(client.Client.RemoteEndPoint.ToString()));
                Id = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
            }
        }
    }
}
