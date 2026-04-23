using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CryptoVault;

public static class steganoServices
{
    public static Image Hide(byte[] data, string password, Image<Rgba32> camoImage)
    {
        // convert password into an int seed
        byte[] passwordHash = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        int seed = BitConverter.ToInt32(passwordHash, 0);
        
        int totalPixels = camoImage.Height * camoImage.Width;
        int[] order = new int[totalPixels];

        // fill array
        for (int i = 0; i < totalPixels; i++)
        {
            order[i] = i;
        }
        
        // instance random class from the password derived seed + order shuffle
        Random r = new Random(seed);
        r.Shuffle(order);

        return null;
    }
}