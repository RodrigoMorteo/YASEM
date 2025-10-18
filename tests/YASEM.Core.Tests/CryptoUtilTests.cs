
using Xunit;
using YASEM.Core.Utilities;
using System.IO;
using System.Security.Cryptography;

namespace YASEM.Core.Tests
{
    public class CryptoUtilTests
    {
        [Fact]
        public void GenerateKey_ShouldCreateKeyFile()
        {
            // Arrange
            var keyFilePath = "test.key";

            // Act
            var key = CryptoUtil.GenerateKey();
            File.WriteAllBytes(keyFilePath, key);

            // Assert
            Assert.True(File.Exists(keyFilePath));

            // Cleanup
            File.Delete(keyFilePath);
        }

        [Fact]
        public void EncryptAndDecrypt_ShouldReturnOriginalString()
        {
            // Arrange
            var key = CryptoUtil.GenerateKey();
            var originalString = "test-string";

            // Act
            var encryptedString = CryptoUtil.Encrypt(originalString, key);
            var decryptedString = CryptoUtil.Decrypt(encryptedString, key);

            // Assert
            Assert.Equal(originalString, decryptedString);
        }

        [Fact]
        public void Decrypt_WithWrongKey_ShouldThrowException()
        {
            // Arrange
            var key1 = CryptoUtil.GenerateKey();
            var key2 = CryptoUtil.GenerateKey();
            var originalString = "test-string";

            // Act
            var encryptedString = CryptoUtil.Encrypt(originalString, key1);

            // Assert
            Assert.Throws<YASEM.Core.Exceptions.DecryptionException>(() => CryptoUtil.Decrypt(encryptedString, key2));
        }
    }
}
