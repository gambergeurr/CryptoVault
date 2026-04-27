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

        uint payloadSize = (uint)data.Length * 8;

        // hide payload size in 8 first bytes for later extraction
        for (int i = 0; i < 8; i++)
        {
            //x y pixel coordinate
            int x = order[i] % camoImage.Width;
            int y = order[i] / camoImage.Width;
            
            Rgba32 pixel = camoImage[x, y];

            // index of the next nibble to hide
            int index = 31 - (4 * i);
            
            // shifting the payloadSize to get the index bit to postition 0 and setting every other bit to 0
            uint bitR = (payloadSize >> index) & 1;
            uint bitG = (payloadSize >> (index - 1)) & 1;
            uint bitB = (payloadSize >> (index - 2)) & 1;
            uint bitA = (payloadSize >> (index - 3)) & 1;
            
            // setting each pixel's lsb to 0 and using OR operator to set it to desired value
            pixel.R = (byte)((pixel.R & 254) | bitR);
            pixel.G = (byte)((pixel.G & 254) | bitG);
            pixel.B = (byte)((pixel.B & 254) | bitB);
            pixel.A = (byte)((pixel.A & 254) | bitA);
        }

        return null;
    }
}