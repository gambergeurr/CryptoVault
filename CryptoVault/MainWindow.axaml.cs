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
        if (sender is modules.LockedModule lockedModule)
        {
            string password = lockedModule.tbxPassword.Text;
            try
            {
                key = CryptoService.GenerateKey(password);
                
                // Try to initialize the ApiManager with the key.
                // It will throw an exception if the file exists and the password (key) is incorrect.
                var apiManager = new modules.ApiManager(key);
                
                // If successful, swap the LockedModule with the real modules.
                tabApi.Content = apiManager;
                tabFiles.Content = new modules.SecureFilesManager(key);
            }
            catch
            {
                lockedModule.tbxPassword.Text = string.Empty;
                lockedModule.tbxPassword.Watermark = "Mot de passe incorrect";
            }
        }
    }
}