using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Image = Avalonia.Controls.Image;

namespace CryptoVault.modules;

public partial class Steganography : UserControl, INotifyPropertyChanged
{
    private Image<Rgba32> CamoImage;
    private string FileName;
    
    public byte[] FileData
    {
        get => field;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged(nameof(FileContent));
            }
        }
    }

    public string FileContent
    {
        get => FileData != null ? Encoding.UTF8.GetString(FileData) : string.Empty;
        set => FileData = value != null ? Encoding.UTF8.GetBytes(value) : Array.Empty<byte>();
    }

    public Steganography()
    {
        InitializeComponent();
        DataContext = this;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            FileData = File.ReadAllBytes(file[0].TryGetLocalPath());
        }
    }

    private async void DownloadFile(object? sender, RoutedEventArgs e)
    {
        if (FileData == null) return;
        
        var topLevel = TopLevel.GetTopLevel(this);

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Enregistrer le fichier",
            SuggestedFileName = FileName
        });

        if (file != null)
        {
            File.WriteAllBytes(file.Path.AbsolutePath, FileData);
        }
    }

    private async void HideData(object? sender, RoutedEventArgs e)
    {
        if (CamoImage == null)
            return;
        if (string.IsNullOrEmpty(tbxPassword.Text))
            return;
        if(FileData == null)
            return;
        
        Image<Rgba32> alteredImage = null;
        byte[] payload = SerializeTuple((FileName ?? "data.txt", FileData));
        
        try
        {
            alteredImage = (Image<Rgba32>)steganoServices.Hide(payload, tbxPassword.Text, CamoImage);
        }
        catch (Exception exception)
        {
            if (exception is ArgumentException)
            {
                await MessageBoxManager.GetMessageBoxStandard("Erreur", "L'image séléctionnée n'est pas assez grande pour cacher le message").ShowWindowDialogAsync(TopLevel.GetTopLevel(this) as Window);
            }
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this);

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Enregistrer l'image stéganographiée",
            DefaultExtension = "png",
            FileTypeChoices = new[] { FilePickerFileTypes.ImagePng },
            SuggestedFileName = "hidden_data.png"
        });

        if (file != null)
        {
            alteredImage.SaveAsPng(file.Path.AbsolutePath);
        }
    }

    private async void UnHide(object? sender, RoutedEventArgs e)
    {
        if (CamoImage == null)
            return;
        if (string.IsNullOrEmpty(tbxPassword.Text))
            return;

        byte[] payload = null;
        try
        {
            payload = steganoServices.Unhide(tbxPassword.Text, CamoImage);
        }
        catch (Exception exception)
        {
            await MessageBoxManager.GetMessageBoxStandard("Erreur", "Erreur lors de l'extraction (probablement mot de passe incrorect)").ShowWindowDialogAsync(TopLevel.GetTopLevel(this) as Window);
            return;
        }
        
        (string, byte[]) file = DeserializeTuple(payload);
        
        FileName = file.Item1;
        FileData = file.Item2;
    }
}