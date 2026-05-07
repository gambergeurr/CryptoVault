using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Image = Avalonia.Controls.Image;

namespace CryptoVault.modules;

public partial class Steganography : UserControl
{
    private SixLabors.ImageSharp.Image<Rgba32> CamoImage;
    private string FileName;
    public Steganography()
    {
        InitializeComponent();
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

    private async void SelectImage(object? sender, RoutedEventArgs e)
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
            CamoImage = Image<Rgba32>.Load<Rgba32>(image[0].TryGetLocalPath());
            ImageBox.Source = new Bitmap(image[0].TryGetLocalPath());
        }
    }

    private async void LoadFile(object? sender, RoutedEventArgs e)
    {
        FilePickerOpenOptions options = new FilePickerOpenOptions
        {
            Title = "Choisir un fichier",
            AllowMultiple =  false
        };
        
        IReadOnlyList<IStorageFile> file = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(options);

        if (file.Count >= 1)
        {
            FileName = file[0].Name;

            tbxContent.Text = Encoding.UTF8.GetString(File.ReadAllBytes(file[0].TryGetLocalPath()));
        }
    }
}