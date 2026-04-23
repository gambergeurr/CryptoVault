using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;

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
            Console.Write(image[0].TryGetLocalPath());
        }
    }
}