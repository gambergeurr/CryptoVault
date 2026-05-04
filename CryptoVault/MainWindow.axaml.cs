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
        string password = (sender as modules.LockedModule).tbxPassword.Text;
        
        key = CryptoService.GenerateKey(password);
    }
}