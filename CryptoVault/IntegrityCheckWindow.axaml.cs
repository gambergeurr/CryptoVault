using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;

namespace CryptoVault;

public partial class IntegrityCheckWindow : Window
{
    private readonly string vaultHash;
    private readonly string fileName;

    public IntegrityCheckWindow()
    {
        vaultHash = string.Empty;
        fileName = string.Empty;
        InitializeComponent();
    }

    public IntegrityCheckWindow(string fileName, string vaultHash) : this()
    {
        fileName = fileName;
        vaultHash = vaultHash;
        
        txtFileName.Text = $"Fichier: {fileName}";
        txtVaultHash.Text = vaultHash;
    }

    private async void BtnSelectLocal_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Sélectionner le fichier local pour comparaison",
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
                    txtStatus.Text = "✅ Intégrité vérifiée : Les fichiers sont identiques.";
                    txtStatus.Foreground = Brushes.Green;
                }
                else
                {
                    txtStatus.Text = "❌ Alerte : Le fichier a été modifié !";
                    txtStatus.Foreground = Brushes.Red;
                }
            }
        }
    }
}