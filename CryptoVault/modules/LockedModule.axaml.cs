using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace CryptoVault.modules;

public partial class LockedModule : UserControl
{
    public event EventHandler PasswordEntered; 
    public LockedModule()
    {
        InitializeComponent();
    }

    private void ValidatePassword(object? sender, RoutedEventArgs e)
    {
        PasswordEntered?.Invoke(this, e);
    }
}