using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Image = Avalonia.Controls.Image;

namespace CryptoVault.modules;

public partial class Steganography : UserControl
{
    public Steganography()
    {
        InitializeComponent();
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        FilePickerOpenOptions options = new FilePickerOpenOptions
        {
            Title = "Choisir un image",
            AllowMultiple =  false,
            FileTypeFilter = new[] {FilePickerFileTypes.ImagePng}
        };
        
        IReadOnlyList<IStorageFile> image = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(options);
        
        if (image.Count >= 1)
        {
            (string, byte[]) testFile = ("music.xm", File.ReadAllBytes("C:/Users/calamerq1/Downloads/mini1111 (1).xm"));
            SixLabors.ImageSharp.Image blbal = steganoServices.Hide(SerializeTuple(testFile), "test", Image<Rgba32>.Load<Rgba32>(image[0].TryGetLocalPath()));

            PngEncoder saveOptions = new PngEncoder {ColorType = PngColorType.RgbWithAlpha};
            blbal.SaveAsPng("C:/Users/calamerq1/Desktop/superimage.png", saveOptions);
        }
    }

    private async void dechache(object? sender, RoutedEventArgs e)
    {
        FilePickerOpenOptions options = new FilePickerOpenOptions
        {
            Title = "Choisir un image",
            AllowMultiple =  false,
            FileTypeFilter = new[] {FilePickerFileTypes.ImagePng}
        };
        
        IReadOnlyList<IStorageFile> image = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(options);
        
        if (image.Count >= 1)
        {
            byte[] test = steganoServices.Unhide("test", Image<Rgba32>.Load<Rgba32>(image[0].TryGetLocalPath()));
            
            (string name, byte[] data) tuple = DeserializeTuple(test);
            File.WriteAllBytes($"C:/Users/calamerq1/Desktop/{tuple.name}", tuple.data);
        }
    }

    private byte[] SerializeTuple((string name, byte[] data) file)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8))
            {
                writer.Write(file.name);
                writer.Write(file.data.Length);
                writer.Write(file.data);
            }
            return ms.ToArray();
        }
    }

    private (string, byte[]) DeserializeTuple(byte[] tuple)
    {
        using (MemoryStream ms = new MemoryStream(tuple))
        {
            using (BinaryReader reader = new BinaryReader(ms, Encoding.UTF8))
            {
                string name = reader.ReadString();
                int dataLenght = reader.ReadInt32(); // how long is the data
                byte[] data = reader.ReadBytes(dataLenght);
                
                return (name, data);
            }
        }
    }
}