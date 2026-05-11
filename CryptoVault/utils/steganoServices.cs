using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CryptoVault;

/// <summary>
/// Provides services for steganography, including hiding and extracting data within an image.
/// </summary>
public static class steganoServices
{
    /// <summary>
    /// Creates a pseudorandom order array based on a password for shuffling pixel processing order.
    /// </summary>
    /// <param name="password">The password used to generate the seed.</param>
    /// <param name="camoImage">The image used for steganography.</param>
    /// <returns>An array representing the shuffled order of pixels.</returns>
    private static int[] CreateOrderArray(string password, Image<Rgba32> camoImage)
    {
        // Convert password into an int seed
        byte[] passwordHash = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        int seed = BitConverter.ToInt32(passwordHash, 0);
        
        int totalPixels = camoImage.Height * camoImage.Width;
        int[] order = new int[totalPixels];

        // Fill array with sequential indices
        for (int i = 0; i < totalPixels; i++)
        {
            order[i] = i;
        }
        
        // Instantiate the random class from the password-derived seed and shuffle the order
        Random r = new Random(seed);
        r.Shuffle(order);
        
        return order;
    }

    /// <summary>
    /// Hides arbitrary data within an image using Least Significant Bit (LSB) steganography.
    /// </summary>
    /// <param name="data">The byte array to hide.</param>
    /// <param name="password">The password used to shuffle the pixel order.</param>
    /// <param name="camoImage">The carrier image.</param>
    /// <returns>The modified image containing the hidden data.</returns>
    public static Image<Rgba32> Hide(byte[] data, string password, Image<Rgba32> camoImage)
    {
        // Check if the data fits in the image (leaving 32 bits for the payload size)
        if (data.Length * 8 > (camoImage.Width * camoImage.Height * 4) - 32)
        {
            throw new ArgumentException("The data is too large for the image");
        }
        
        int[] order = CreateOrderArray(password, camoImage);

        uint payloadSize = (uint)data.Length * 8;

        // Hide the payload size in the first 8 pixels (8 * 4 channels = 32 bits) for later extraction
        for (int i = 0; i < 8; i++)
        {
            // X, Y pixel coordinate
            int x = order[i] % camoImage.Width;
            int y = order[i] / camoImage.Width;
            
            Rgba32 pixel = camoImage[x, y];

            // Index of the next bit to hide
            int index = 31 - (4 * i);
            
            // Shift the payloadSize to get the target bit to position 0 and set every other bit to 0
            uint bitR = (payloadSize >> index) & 1;
            uint bitG = (payloadSize >> (index - 1)) & 1;
            uint bitB = (payloadSize >> (index - 2)) & 1;
            uint bitA = (payloadSize >> (index - 3)) & 1;
            
            // Set each pixel channel's LSB to 0 using AND 254, then use OR to set it to the desired bit value
            pixel.R = (byte)((pixel.R & 254) | bitR);
            pixel.G = (byte)((pixel.G & 254) | bitG);
            pixel.B = (byte)((pixel.B & 254) | bitB);
            pixel.A = (byte)((pixel.A & 254) | bitA);
            
            camoImage[x, y] = pixel;
        }

        for (int i = 0; i < payloadSize / 4; i++)
        {
            // X, Y pixel coordinate
            int x = order[i + 8] % camoImage.Width;
            int y = order[i + 8] / camoImage.Width;
            
            Rgba32 pixel = camoImage[x, y];
            
            // Index of the next bit in the current byte
            int index = 7 - ((i * 4) % 8);
    
            // Shift the current byte to get the target bit to position 0 and set every other bit to 0
            uint bitR = (uint)((data[i / 2] >> index) & 1);
            uint bitG = (uint)((data[i / 2] >> (index - 1)) & 1);
            uint bitB = (uint)((data[i / 2] >> (index - 2)) & 1);
            uint bitA = (uint)((data[i / 2] >> (index - 3)) & 1);
            
            // Set each pixel channel's LSB to 0 using AND 254, then use OR to set it to the desired bit value
            pixel.R = (byte)((pixel.R & 254) | bitR);
            pixel.G = (byte)((pixel.G & 254) | bitG);
            pixel.B = (byte)((pixel.B & 254) | bitB);
            pixel.A = (byte)((pixel.A & 254) | bitA);
            
            camoImage[x, y] = pixel;
        }

        return camoImage;
    }

    /// <summary>
    /// Extracts hidden data from an image.
    /// </summary>
    /// <param name="password">The password used to recover the pixel processing order.</param>
    /// <param name="camoImage">The image containing the hidden data.</param>
    /// <returns>A byte array representing the extracted data.</returns>
    public static byte[] Unhide(string password, Image<Rgba32> camoImage)
    {
        int[] order = CreateOrderArray(password, camoImage);

        uint payloadSize = 0;
        for (int i = 0; i < 8; i++)
        {
            // X, Y pixel coordinate
            int x = order[i] % camoImage.Width;
            int y = order[i] / camoImage.Width;
            
            Rgba32 pixel = camoImage[x, y];
            
            // Reconstruct the integer from Most Significant Bit (MSB) to Least Significant Bit (LSB)
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
            
            // Get every LSB of the current pixel's channels
            uint bitR = (uint)(pixel.R & 1);
            uint bitG = (uint)(pixel.G & 1);
            uint bitB = (uint)(pixel.B & 1);
            uint bitA = (uint)(pixel.A & 1);

            // Index of the next bit to write in the current byte
            int index = 7 - ((i * 4) % 8);

            // Shift the LSBs to their correct positions and combine them into the byte
            data[i / 2] |= (byte)(bitR << index);
            data[i / 2] |= (byte)(bitG << (index - 1));
            data[i / 2] |= (byte)(bitB << (index - 2));
            data[i / 2] |= (byte)(bitA << (index - 3));
        }
        
        return data;
    }
}