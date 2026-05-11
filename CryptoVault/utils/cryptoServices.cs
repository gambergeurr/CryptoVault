using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Provides cryptographic services such as key generation, encryption, and decryption using AES-256.
/// </summary>
public static class CryptoService
{
    /// <summary>
    /// Generates an encryption key from a given password using PBKDF2.
    /// </summary>
    /// <param name="password">The password used to generate the key.</param>
    /// <returns>A 32-byte (256-bit) encryption key.</returns>
    public static byte[] GenerateKey(string password)
    {
        // Throw an error if no password is entered
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("The password cannot be empty."); // Translated from "Le mot de passe ne peut pas être vide."
            
        byte[] StaticSalt = Encoding.UTF8.GetBytes("I5O=O2uf0SQ="); // Create a static salt for PBKDF2
        byte[] key = new byte[32]; // Initialize an empty 32-byte key for AES-256

        Rfc2898DeriveBytes.Pbkdf2(password, StaticSalt, key, 100000, HashAlgorithmName.SHA256); // Derive key and copy to 'key' variable
        return key;
    }

    /// <summary>
    /// Encrypts data using AES-256 in CBC mode.
    /// </summary>
    /// <param name="data">The plaintext data to encrypt.</param>
    /// <param name="key">The 32-byte encryption key.</param>
    /// <returns>An array containing the Initialization Vector (IV) followed by the encrypted data.</returns>
    public static byte[] Encrypt(byte[] data, byte[] key)
    {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();

                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {

                    // Encrypt the data
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(data, 0, data.Length);

                    // Add the IV to the beginning of the result for later decryption
                    byte[] finalResult = aes.IV.Concat(encryptedBytes).ToArray();
                    
                    return finalResult;
                }
            }
    }

    /// <summary>
    /// Decrypts data using AES-256 in CBC mode.
    /// </summary>
    /// <param name="encryptedData">The encrypted data, prepended with the Initialization Vector (IV).</param>
    /// <param name="key">The 32-byte encryption key.</param>
    /// <returns>The decrypted plaintext data.</returns>
    public static byte[] Decrypt(byte[] encryptedData, byte[] key)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;

            byte[] iv = new byte[16];
            Array.Copy(encryptedData, 0, iv, 0, iv.Length); // Extract the first 16 bytes (the IV)
            aes.IV = iv;

            int dataLength = encryptedData.Length - 16;

            using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV)) // Create decryptor from the key and the IV
            {
                byte[] decryptedData = decryptor.TransformFinalBlock(encryptedData, 16, dataLength); // Decrypt the data

                return decryptedData;
            }
        }
    }

    /// <summary>
    /// Computes the SHA-256 hash of the provided data.
    /// </summary>
    /// <param name="data">The input data.</param>
    /// <returns>The hexadecimal string representation of the hash.</returns>
    public static string Hash(byte[] data)
    {
        byte[] hashBytes = SHA256.HashData(data);
        return Convert.ToHexString(hashBytes);
    }
}