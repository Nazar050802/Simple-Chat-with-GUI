using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace ServerSide
{
    internal class Room
    {
        public string Name { get; set; }

        private string Password { get; set; }

        public ConcurrentBag<string> UsersInRoom { get; set; }

        public Room(string name, string password) { 

            Name = name;   
            UsersInRoom = new ConcurrentBag<string>();

            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
                Password = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
            }
        }   

        public bool ComparePassword(string password)
        {
            string tempPassword = "";
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
                tempPassword = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
            }

            return (tempPassword == Password);
        }
    }
}
