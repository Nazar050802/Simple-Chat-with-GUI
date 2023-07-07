using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace ServerSide
{
    public class User
    {
        public string Id { get; set; }

        public string SecureCode { get; set; }

        public string Name { get; set; }

        public RSAGenerating rsaGeneratingServer  {get; set; }

        public RSAGenerating rsaGeneratingClient { get; set; }

        public TcpClient TcpConnection { get; set; }

        public string CurrentRoomName { get; set; }

        public User(TcpClient client) {

            TcpConnection = client;
            rsaGeneratingClient = new RSAGenerating();
            rsaGeneratingServer = new RSAGenerating();

            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(client.Client.RemoteEndPoint.ToString()));
                Id = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                Random random = new Random();
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes($"{random.Next(1, 16385)}{Id}"));
                string tempHash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);

                SecureCode = $"{tempHash.Substring(0, 3)}{tempHash.Substring(tempHash.Length - 3, 3)}";
            }
        }
    }
}
