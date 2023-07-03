using ClientSide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class SimpleLogsTests
    {
        private string testFileName;
        private string testFilePath;

        [SetUp]
        public void Setup()
        {
            // Create a temp test file
            testFileName = $"test_log_{DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss")}.txt";
            testFilePath = Path.Combine(Environment.CurrentDirectory, testFileName);
        }

        [TearDown]
        public void TearDown()
        {
            // Delete the temporary test file after each test
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
        }

        [Test]
        public void CreateLogFile_FileDoesNotExist_LogFileCreated()
        {
            // Act
            bool result = SimpleLogs.CreateLogFile(testFilePath);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(testFilePath));
        }

        [Test]
        public void CreateLogFile_FileExists_LogFileNotCreated()
        {
            // Arrange
            File.Create(testFilePath).Close();

            // Act
            bool result = SimpleLogs.CreateLogFile();

            // Assert

            // Creating an existing file should not cause an error
            Assert.IsTrue(result); 
            Assert.IsTrue(File.Exists(testFilePath));
        }

        [Test]
        public void WriteToFile_FileExists_TextAppendedToFile()
        {
            // Arrange
            SimpleLogs.CreateLogFile(testFilePath);
            string textToWrite = "Test log";

            // Act
            bool result = SimpleLogs.WriteToFile(textToWrite, testFilePath);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual($"[{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}] {textToWrite}", File.ReadAllText(testFilePath).TrimEnd());
        }
    }
}
