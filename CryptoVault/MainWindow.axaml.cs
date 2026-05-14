using System;
using Avalonia.Controls;
using Avalonia.Media;

namespace CryptoVault;

/// <summary>
/// The main window of the application that hosts the different modules.
/// </summary>
public partial class MainWindow : Window
{
    private byte[] key;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Attempts to decrypt the vault using the password entered in the locked module.
    /// </summary>
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