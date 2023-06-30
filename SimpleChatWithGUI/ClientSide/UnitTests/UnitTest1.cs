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
            rsaGenerator.GenerateKeys();
            Assert.IsNotNull(rsaGenerator.PublicKey);
        }

        [Test]
        public void EncryptRawData_DecryptRawData_MatchOriginalData()
        {
            byte[] originalData = { 0x01, 0x02, 0x03, 0x04, 0x05 };
            byte[] encryptedData = rsaGenerator.EncryptRawData(originalData);
            byte[] decryptedData = rsaGenerator.DecryptRawData(encryptedData);
            CollectionAssert.AreEqual(originalData, decryptedData);
        }

        [Test]
        public void EncryptString_DecryptIntoString_MatchOriginalData()
        {
            string originalData = "Hello, World!";
            byte[] encryptedData = rsaGenerator.EncryptString(originalData);
            string decryptedData = rsaGenerator.DecryptIntoString(encryptedData);

            Assert.AreEqual(originalData, decryptedData);
        }
    }
}