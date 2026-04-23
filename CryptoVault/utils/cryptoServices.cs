using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;


public static class CryptoService
{
    private static readonly byte[] StaticSalt = Encoding.UTF8.GetBytes("I5O=O2uf0SQ=");

    public static byte[] GenerateKey(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Le mot de passe ne peut pas être vide.");
        byte[] key = new byte[32];

        Rfc2898DeriveBytes.Pbkdf2(password, StaticSalt, key, 100000, HashAlgorithmName.SHA256);
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

                    // 2. Chiffrer
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(data, 0, data.Length);

                    // 3. Combiner IV + ContenuChiffré
                    byte[] finalResult = aes.IV.Concat(encryptedBytes).ToArray();

                    // 4. Retourner en Base64
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
            Array.Copy(encryptedData, 0, iv, 0, iv.Length);
            aes.IV = iv;

            int dataLength = encryptedData.Length - 16;

            using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            {
                byte[] decryptedData = decryptor.TransformFinalBlock(encryptedData, 16, dataLength);

                return decryptedData;
            }
        }
    }
}
