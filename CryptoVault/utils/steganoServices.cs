using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CryptoVault;

public static class steganoServices
{
    private static int[] CreateOrderArray(string password, Image<Rgba32> camoImage)
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
        
        return order;
    }
    public static Image Hide(byte[] data, string password, Image<Rgba32> camoImage)
    {
        int[] order = CreateOrderArray(password, camoImage);

        uint payloadSize = (uint)data.Length * 8;

        // hide payload size in 8 first pixels (8 * 4 = 32bit) for later etraction
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
            
            camoImage[x, y] = pixel;
        }

        for (int i = 0; i < payloadSize / 4; i++)
        {
            //x y pixel coordinate
            int x = order[i + 8] % camoImage.Width;
            int y = order[i + 8] / camoImage.Width;
            
            Rgba32 pixel = camoImage[x, y];
            
            // index of the next nibble in the [i / 2]th byte
            int index = 7 - ((i * 4) % 8);
    
            // getting the indexed byte to position 0 and setting every other to 0
            uint bitR = (uint)((data[i / 2] >> index) & 1);
            uint bitG = (uint)((data[i / 2] >> (index - 1)) & 1);
            uint bitB = (uint)((data[i / 2] >> (index - 2)) & 1);
            uint bitA = (uint)((data[i / 2] >> (index - 3)) & 1);
            
            // setting each pixel's lsb to 0 and using OR operator to set it to desired value
            pixel.R = (byte)((pixel.R & 254) | bitR);
            pixel.G = (byte)((pixel.G & 254) | bitG);
            pixel.B = (byte)((pixel.B & 254) | bitB);
            pixel.A = (byte)((pixel.A & 254) | bitA);
            
            camoImage[x, y] = pixel;
        }

        return camoImage;
    }

    public static byte[] Unhide(string password, Image<Rgba32> camoImage)
    {
        int[] order = CreateOrderArray(password, camoImage);

        uint payloadSize = 0;
        for (int i = 0; i < 8; i++)
        {
            //x y pixel coordinate
            int x = order[i] % camoImage.Width;
            int y = order[i] / camoImage.Width;
            
            Rgba32 pixel = camoImage[x, y];
            
            // building the integer from msb to lsb
            payloadSize = (uint)((payloadSize << 1) | pixel.R & 1);
            payloadSize = (uint)((payloadSize << 1) | pixel.G & 1);
            payloadSize = (uint)((payloadSize << 1) | pixel.B & 1);
            payloadSize = (uint)((payloadSize << 1) | pixel.A & 1);
        }

        byte[] data = new byte[payloadSize / 8];
        
        for (int i = 0; i < payloadSize / 4; i++)
        {
            int x = order[i + 8] % camoImage.Width;
            int y = order[i + 8] / camoImage.Width;
            
            Rgba32 pixel = camoImage[x, y];
            
            // get every lsb of the pixel
            uint bitR = (uint)(pixel.R & 1);
            uint bitG = (uint)(pixel.G & 1);
            uint bitB = (uint)(pixel.B & 1);
            uint bitA = (uint)(pixel.A & 1);

            // index of the next nibble to write in the [i / 8] byte
            int index = 7 - ((i * 4) % 8);

            // writing the next msb and shifting
            data[i / 2] |= (byte)(bitR << index);
            data[i / 2] |= (byte)(bitG << (index - 1));
            data[i / 2] |= (byte)(bitB << (index - 2));
            data[i / 2] |= (byte)(bitA << (index - 3));
        }
        
        return data;
    }
}