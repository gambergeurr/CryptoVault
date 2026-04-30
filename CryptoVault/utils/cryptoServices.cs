using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


public static class CryptoService
{
    public static byte[] GenerateKey(string password)
    {
        // throw error if no password is entered
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Le mot de passe ne peut pas être vide.");
        byte[] StaticSalt = Encoding.UTF8.GetBytes("I5O=O2uf0SQ="); // create salt for sha-256
        byte[] key = new byte[32]; // initialize empty key

        Rfc2898DeriveBytes.Pbkdf2(password, StaticSalt, key, 100000, HashAlgorithmName.SHA256); // copy generated key to 'key' variable
        return key;
    }

    public static byte[] Encrypt(byte[] data, byte[] key)
    {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();

                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {

                    // Encrypt
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(data, 0, data.Length);

                    // add IV to for later decryption
                    byte[] finalResult = aes.IV.Concat(encryptedBytes).ToArray();
                    
                    return finalResult;
                }
            }
    }

    public static byte[] Decrypt(byte[] encryptedData, byte[] key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;

            byte[] iv = new byte[16];
            Array.Copy(encryptedData, 0, iv, 0, iv.Length); // extract the first 16 byte (IV)
            aes.IV = iv;

            int dataLength = encryptedData.Length - 16;

            using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV)) // create decryptor form the key and the IV
            {
                byte[] decryptedData = decryptor.TransformFinalBlock(encryptedData, 16, dataLength); // decrypt

                return decryptedData;
            }
        }
    }
}
