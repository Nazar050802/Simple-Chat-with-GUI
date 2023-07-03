namespace UnitTests
{
    [TestFixture]
    public class RSAGeneratingTests
    {
        private RSAGenerating rsaGenerator;

        [SetUp]
        public void SetUp()
        {
            rsaGenerator = new RSAGenerating();
        }

        [Test]
        public void GenerateKeys_PublicKeyNotNull()
        {
            // Act
            rsaGenerator.GenerateKeys();

            // Assert
            Assert.IsNotNull(rsaGenerator.PublicKey);
        }

        [Test]
        public void EncryptRawData_DecryptRawData_MatchOriginalData()
        {
            // Arrange
            byte[] originalData = { 0xA1, 0xA2, 0xA3, 0xA4, 0xA5 };
            byte[] encryptedData;

            // Act
            encryptedData = rsaGenerator.EncryptRawData(originalData);
            byte[] decryptedData = rsaGenerator.DecryptRawData(encryptedData);

            // Assert
            CollectionAssert.AreEqual(originalData, decryptedData);
        }

        [Test]
        public void EncryptString_DecryptIntoString_MatchOriginalData()
        {
            // Arrange
            string originalData = "Hello, MFF !!!";
            byte[] encryptedData;
            string decryptedData;

            // Act
            encryptedData = rsaGenerator.EncryptString(originalData);
            decryptedData = rsaGenerator.DecryptIntoString(encryptedData);

            // Assert
            Assert.AreEqual(originalData, decryptedData);
        }
    }
}