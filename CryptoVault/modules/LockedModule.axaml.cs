using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace CryptoVault.modules;

/// <summary>
/// A wrapper user control that prompts for a password before granting access to its content.
/// </summary>
public partial class LockedModule : UserControl
{
    /// <summary>
    /// Occurs when a password has been entered and validation is requested.
    /// </summary>
    public event EventHandler PasswordEntered; 

    /// <summary>
    /// Initializes a new instance of the <see cref="LockedModule"/> class.
    /// </summary>
    public LockedModule()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Event handler for the validate password button click. Invokes the PasswordEntered event.
    /// </summary>
    private void ValidatePassword(object? sender, RoutedEventArgs e)
    {
        PasswordEntered?.Invoke(this, e);
    }
}