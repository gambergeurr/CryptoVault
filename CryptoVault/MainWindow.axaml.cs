using System;
using Avalonia.Controls;
using Avalonia.Media;

namespace CryptoVault;

public partial class MainWindow : Window
{
    private byte[] key;
    public MainWindow()
    {
        InitializeComponent();
    }

    private void TryDecrypt(object? sender, EventArgs e)
    {
        modules.LockedModule lockedModule = (sender as modules.LockedModule);
        
        string password = lockedModule.tbxPassword.Text;
        try
        {
            key = CryptoService.GenerateKey(password);
            
            var apiManager = new modules.ApiManager(key);
            var SecureFiles = new modules.SecureFilesManager(key);
            
            tabApi.Content = apiManager;
            tabFiles.Content = SecureFiles;
        }
        catch
        {
            lockedModule.tbxPassword.Text = string.Empty;
            lockedModule.tbxPassword.Watermark = "Mot de passe incorrect";
        }
    }
}