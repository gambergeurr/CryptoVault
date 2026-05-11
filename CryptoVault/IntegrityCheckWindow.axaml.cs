using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;

namespace CryptoVault;

/// <summary>
/// A window for checking the integrity of a file against a hash stored in the vault.
/// </summary>
public partial class IntegrityCheckWindow : Window
{
    private readonly string vaultHash;
    private readonly string fileName;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrityCheckWindow"/> class.
    /// </summary>
    public IntegrityCheckWindow()
    {
        vaultHash = string.Empty;
        fileName = string.Empty;
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IntegrityCheckWindow"/> class with a specific file and hash.
    /// </summary>
    /// <param name="fileName">The name of the file being checked.</param>
    /// <param name="vaultHash">The hash of the file as stored in the vault.</param>
    public IntegrityCheckWindow(string fileName, string vaultHash) : this()
    {
        this.fileName = fileName;
        this.vaultHash = vaultHash;
        
        txtFileName.Text = $"File: {fileName}"; // Translated from "Fichier: "
        txtVaultHash.Text = vaultHash;
    }

    /// <summary>
    /// Event handler for selecting a local file to compare its hash with the vault hash.
    /// </summary>
    private async void BtnSelectLocal_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select local file for comparison", // Translated from "Sélectionner le fichier local pour comparaison"
            AllowMultiple = false
        });

        if (files.Count > 0)
        {
            var file = files[0];
            if (file.TryGetLocalPath() is string localPath)
            {
                byte[] localData = await File.ReadAllBytesAsync(localPath);
                string localHash = CryptoService.Hash(localData);

                txtLocalHash.Text = localHash;
                panelLocalResult.IsVisible = true;

                if (localHash == vaultHash)
                {
                    txtStatus.Text = "✅ The files are identical."; // Translated from "✅ Les fichiers sont identiques."
                    txtStatus.Foreground = Brushes.Green;
                }
                else
                {
                    txtStatus.Text = "❌ The file has been modified."; // Translated from "❌ Le fichier a été modifié."
                    txtStatus.Foreground = Brushes.Red;
                }
            }
        }
    }
}