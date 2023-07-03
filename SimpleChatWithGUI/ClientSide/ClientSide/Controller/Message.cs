using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientSide
{
    public class Message
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public string Text { get; set; }

        public bool SenderOrReceiver { get; set; }

        public string Time { get; set; }

        public Message(string username, string text, bool senderOrReceiver) {
            Username = username;
            Text = text;
            SenderOrReceiver = senderOrReceiver;
            Time = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");

            using (System.Security.Cryptography.SHA1 sha1 = System.Security.Cryptography.SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes($"{Time};{Username};{Text}"));
                Id = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
            }
        }

    }
}
