using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace ServerSide
{
    public class Room
    {
        public string Name { get; set; }

        private string Password { get; set; }

        public ConcurrentBag<string> UsersInRoom { get; set; }

        /// <summary>
        /// Constructor initialize a new instance of the Room class with the specified name and password
        /// </summary>
        /// <param name="name">The room name </param>
        /// <param name="password">The room password</param>
        public Room(string name, string password) { 

            Name = name;   
            UsersInRoom = new ConcurrentBag<string>();

            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
                Password = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
            }
        }

        /// <summary>
        /// Compare the specified password with the hashed password of the room
        /// </summary>
        /// <param name="password">The password to compare</param>
        /// <returns>True if the passwords match, otherwise false</returns>
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
