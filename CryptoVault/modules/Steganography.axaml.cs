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

/// <summary>
/// User control for the Steganography module, allowing users to hide and extract files within images.
/// </summary>
public partial class Steganography : UserControl, INotifyPropertyChanged
{
    private Image<Rgba32> CamoImage;
    private string FileName;
    
    /// <summary>
    /// Gets or sets the raw file data to be hidden or extracted.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the file content as a UTF-8 string. Used for data binding.
    /// </summary>
    public string FileContent
    {
        get => FileData != null ? Encoding.UTF8.GetString(FileData) : string.Empty;
        set => FileData = value != null ? Encoding.UTF8.GetBytes(value) : Array.Empty<byte>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Steganography"/> class.
    /// </summary>
    public Steganography()
    {
        InitializeComponent();
        DataContext = this;
    }

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Serializes a file name and its binary data into a byte array.
    /// </summary>
    /// <param name="file">A tuple containing the file name and its byte data.</param>
    /// <returns>A serialized byte array.</returns>
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

    /// <summary>
    /// Deserializes a byte array back into a file name and its binary data.
    /// </summary>
    /// <param name="tuple">The serialized byte array.</param>
    /// <returns>A tuple containing the file name and its byte data.</returns>
    private (string, byte[]) DeserializeTuple(byte[] tuple)
    {
        using (MemoryStream ms = new MemoryStream(tuple))
        {
            using (BinaryReader reader = new BinaryReader(ms, Encoding.UTF8))
            {
                string name = reader.ReadString();
                int dataLenght = reader.ReadInt32(); // The length of the data
                byte[] data = reader.ReadBytes(dataLenght);
                
                return (name, data);
            }
        }
    }

    /// <summary>
    /// Opens a file picker dialog for the user to select the carrier image.
    /// </summary>
    private async void SelectImage(object? sender, RoutedEventArgs e)
    {
        FilePickerOpenOptions options = new FilePickerOpenOptions
        {
            Title = "Choose an image", // Translated from "Choisir un image"
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

    /// <summary>
    /// Opens a file picker dialog for the user to select a file to be hidden.
    /// </summary>
    private async void LoadFile(object? sender, RoutedEventArgs e)
    {
        FilePickerOpenOptions options = new FilePickerOpenOptions
        {
            Title = "Choose a file", // Translated from "Choisir un fichier"
            AllowMultiple =  false
        };
        
        IReadOnlyList<IStorageFile> file = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(options);

        if (file.Count >= 1)
        {
            FileName = file[0].Name;
            FileData = File.ReadAllBytes(file[0].TryGetLocalPath());
        }
    }

    /// <summary>
    /// Opens a save file dialog to download the currently loaded or extracted file.
    /// </summary>
    private async void DownloadFile(object? sender, RoutedEventArgs e)
    {
        if (FileData == null) return;
        
        var topLevel = TopLevel.GetTopLevel(this);

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save the file", // Translated from "Enregistrer le fichier"
            SuggestedFileName = FileName
        });

        if (file != null)
        {
            File.WriteAllBytes(file.Path.AbsolutePath, FileData);
        }
    }

    /// <summary>
    /// Executes the steganography hiding process using the provided password and image.
    /// </summary>
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
                // Translated error message: "The selected image is not large enough to hide the message"
                await MessageBoxManager.GetMessageBoxStandard("Error", "The selected image is not large enough to hide the message").ShowWindowDialogAsync(TopLevel.GetTopLevel(this) as Window);
            }
            return;
        }

        var topLevel = TopLevel.GetTopLevel(this);

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save the steganographic image", // Translated from "Enregistrer l'image stéganographiée"
            DefaultExtension = "png",
            FileTypeChoices = new[] { FilePickerFileTypes.ImagePng },
            SuggestedFileName = "hidden_data.png"
        });

        if (file != null)
        {
            alteredImage.SaveAsPng(file.Path.AbsolutePath);
        }
    }

    /// <summary>
    /// Extracts hidden data from the loaded image using the provided password.
    /// </summary>
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
            // Translated error message: "Error during extraction (probably incorrect password)"
            await MessageBoxManager.GetMessageBoxStandard("Error", "Error during extraction (probably incorrect password)").ShowWindowDialogAsync(TopLevel.GetTopLevel(this) as Window);
            return;
        }
        
        (string, byte[]) file = DeserializeTuple(payload);
        
        FileName = file.Item1;
        FileData = file.Item2;
    }
}