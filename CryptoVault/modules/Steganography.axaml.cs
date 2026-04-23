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
using SixLabors.ImageSharp.PixelFormats;

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
            steganoServices.Hide(Encoding.UTF8.GetBytes("test"), "test", Image<Rgba32>.Load<Rgba32>(image[0].TryGetLocalPath()));
        }
    }
}