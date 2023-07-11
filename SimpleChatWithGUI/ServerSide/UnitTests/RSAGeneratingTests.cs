namespace UnitTests
{
    [TestFixture]
    public class RSAGeneratingTests
    {
        private RSAGenerating rsaGenerator;

        /// <summary>
        /// Setup method for the test fixture
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            rsaGenerator = new RSAGenerating();
        }

        /// <summary>
        /// Test to check that GenerateKeys method generates non null public key
        /// </summary>
        [Test]
        public void GenerateKeys_PublicKeyNotNull()
        {
            // Act
            rsaGenerator.GenerateKeys();

            // Assert
            Assert.IsNotNull(rsaGenerator.PublicKey);
        }

        /// <summary>
        /// Test to check that EncryptRawData and DecryptRawData methods correctly encrypt and decrypt raw data
        /// </summary>
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

        /// <summary>
        /// Test to check that EncryptString and DecryptIntoString methods correctly encrypt and decrypt a string
        /// </summary>
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