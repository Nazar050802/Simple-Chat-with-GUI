using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class RoomTests
    {
        /// <summary>
        /// Test to check the ComparePassword method of the Room class when a valid password is provided
        /// </summary>
        [Test]
        public void ComparePassword_ValidPassword_ReturnsTrue()
        {
            // Arrange
            string password = "password";
            Room room = new Room("Room", password);

            // Act
            bool result = room.ComparePassword(password);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Test to check the ComparePassword method of the Room class when an invalid password is provided
        /// </summary>
        [Test]
        public void ComparePassword_InvalidPassword_ReturnsFalse()
        {
            // Arrange
            string password = "password";
            string invalidPassword = "wrongpassword";
            Room room = new Room("Room", password);

            // Act
            bool result = room.ComparePassword(invalidPassword);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Test to check the initialization of the UsersInRoom property of the Room class.
        /// </summary>
        [Test]
        public void UsersInRoom_InitializedEmpty()
        {
            // Arrange
            string password = "password";
            Room room = new Room("Room", password);

            // Act
            ConcurrentBag<string> users = room.UsersInRoom;

            // Assert
            Assert.IsNotNull(users);
            Assert.AreEqual(0, users.Count);
        }
    }
}
