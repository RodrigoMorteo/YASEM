using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YASEM.Core.Utilities
{
    public static class CryptoUtil
    {
        public static byte[] GenerateKey()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] key = new byte[32]; // 256 bits
                rng.GetBytes(key);
                return key;
            }
        }

        public static string Encrypt(string plainText, byte[] key)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.GenerateIV(); // Generate a new IV for each encryption

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    // Prepend IV to the ciphertext
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);

                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public static string Decrypt(string base64CipherText, byte[] key)
        {
            byte[] cipherTextBytesWithIv = Convert.FromBase64String(base64CipherText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;

                // Extract IV from the beginning of the ciphertext
                byte[] iv = new byte[aesAlg.BlockSize / 8];
                Array.Copy(cipherTextBytesWithIv, 0, iv, 0, iv.Length);
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream())
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                    {
                        // Write ciphertext (excluding IV) to the CryptoStream
                        csDecrypt.Write(cipherTextBytesWithIv, iv.Length, cipherTextBytesWithIv.Length - iv.Length);
                    }
                    return Encoding.UTF8.GetString(msDecrypt.ToArray());
                }
            }
        }
    }
}