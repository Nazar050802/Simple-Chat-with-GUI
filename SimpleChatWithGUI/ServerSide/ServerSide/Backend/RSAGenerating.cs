using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ServerSide
{
    public class RSAGenerating
    {
        public string PublicKey { get; protected set; }

        protected RSAParameters KeyParams { get; set; }

        /// <summary>
        /// Constructor generate the RSA keys
        /// </summary>
        public RSAGenerating()
        {
            GenerateKeys();
        }

        /// <summary>
        /// Generate the RSA public and private keys and convert the public key to a string
        /// </summary>
        public void GenerateKeys()
        {
            // Generate private and public keys and also convert PublicKey in string
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                PublicKey = rsa.ToXmlString(false);
                KeyParams = rsa.ExportParameters(true);
            }
        }

        /// <summary>
        /// Set the public key from the provided string
        /// </summary>
        /// <param name="publicKey">The public key as a string</param>
        public void SetPublicKeyFromString(string publicKey)
        {
            // Set public key from string without setting a private key
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(KeyParams);
                rsa.FromXmlString(publicKey);
                KeyParams = rsa.ExportParameters(false);
            };
        }

        /// <summary>
        /// Encrypts the provided data in bytes using the public key
        /// </summary>
        /// <param name="data">The data to be encrypted</param>
        /// <returns>The encrypted data as a byte array</returns>
        public byte[] EncryptRawData(byte[] data)
        {
            // Using the public key encrypt data
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(KeyParams);
                return rsa.Encrypt(data, true);
            }
        }

        /// <summary>
        /// Decrypt the provided encrypted data using the private key
        /// </summary>
        /// <param name="data">The encrypted data to be decrypted</param>
        /// <returns>The decrypted data as a byte array</returns>
        public byte[] DecryptRawData(byte[] data)
        {
            // Using the private key decrypt data
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(KeyParams);
                return rsa.Decrypt(data, true);
            }
        }

        /// <summary>
        /// Encrypt the provided string data using the public key
        /// </summary>
        /// <param name="data">The string data to be encrypted</param>
        /// <returns>The encrypted data as a byte array</returns>
        public byte[] EncryptString(string data)
        {
            int blockSize = Constants.BufferSize;
            int actualSize = blockSize / 2;
            int blockCount = blockSize / Constants.EncryptSizeBytes;

            // Separate data into blocks, then encrypt them and then combine this blocks into one block
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            Array.Resize(ref dataBytes, blockSize);
            byte[][] blocks = new byte[blockCount][];

            for (int i = 0; i < blockCount; i++)
            {
                blocks[i] = new byte[actualSize / blockCount];
                Array.Copy(dataBytes, i * (actualSize / blockCount), blocks[i], 0, actualSize / blockCount);
            }

            for (int i = 0; i < blockCount; i++)
            {
                blocks[i] = EncryptRawData(blocks[i]);
            }

            byte[] result = new byte[blockSize];
            int currentIndex = 0;
            foreach (byte[] block in blocks)
            {
                Array.Copy(block, 0, result, currentIndex, block.Length);
                currentIndex += block.Length;
            }

            return result;
        }

        /// <summary>
        /// Decrypt the provided encrypted data and returns it as a string
        /// </summary>
        /// <param name="data">The encrypted data to be decrypted</param>
        /// <returns>The decrypted data as a string</returns>
        public string DecryptIntoString(byte[] data)
        {
            int blockSize = Constants.EncryptSizeBytes;
            int blockCount = data.Length / blockSize;
            byte[][] blocks = new byte[blockCount][];

            // Separate data into blocks, then decrypt them and then combine this blocks into one block
            for (int i = 0; i < blockCount; i++)
            {
                blocks[i] = new byte[blockSize];
                Array.Copy(data, i * blockSize, blocks[i], 0, blockSize);
            }

            for (int i = 0; i < blockCount; i++)
            {
                blocks[i] = DecryptRawData(blocks[i]);
            }

            int nonZeroCount = 0;
            foreach (byte[] block in blocks)
            {
                for (int i = 0; i < block.Length; i++)
                {
                    if (block[i] != 0)
                        nonZeroCount++;
                }
            }

            byte[] result = new byte[nonZeroCount];
            int currentIndex = 0;
            foreach (byte[] block in blocks)
            {
                for (int i = 0; i < block.Length; i++)
                {
                    if (block[i] != 0)
                    {
                        result[currentIndex] = block[i];
                        currentIndex++;
                    }
                }
            }

            return Encoding.UTF8.GetString(result);
        }
    }

}
