using System;
using System.Collections.Generic;
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
            SixLabors.ImageSharp.Image blbal = steganoServices.Hide(Encoding.UTF8.GetBytes("je suis une data2"), "test", Image<Rgba32>.Load<Rgba32>(image[0].TryGetLocalPath()));

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
            steganoServices.Unhide("test", Image<Rgba32>.Load<Rgba32>(image[0].TryGetLocalPath()));
        }
    }
}