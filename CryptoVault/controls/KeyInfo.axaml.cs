using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace CryptoVault.controls;

public partial class KeyInfo : UserControl
{
    public string Title {
        get;
        set
        {
            field = value;
        }
    }

    public bool DisplayMode
    {
        get;
        set
        {
            field = value;
            
        }
    }
    public event EventHandler? KeyChanged;
    public event EventHandler? KeyDeleted;


    public KeyInfo()
    {
        DataContext = this;
        InitializeComponent();
        DisplayMode = true;
    }
    
    private void BtnEdit_OnClick(object? sender, RoutedEventArgs e)
    {
        DisplayMode = !DisplayMode;
        btnEdit.IsVisible = false;
        btnValider.IsVisible = true;
        btnDelete.IsVisible = true;
    }
    
    private void BtnValider_OnClick(object? sender, RoutedEventArgs e)
    {
        DisplayMode = !DisplayMode;
        btnEdit.IsVisible = true;
        btnValider.IsVisible = false;
        btnDelete.IsVisible = false;
        
        KeyChanged.Invoke(this, EventArgs.Empty);
    }
    
    private void BtnDelete_OnClick(object? sender, RoutedEventArgs e)
    {
        tbxTitle.Text = null;
        tbxKey.TbxText = null;
        tbxDescription.Text = null;
        tbxDate.Text = null;
        
        DisplayMode = !DisplayMode;
        btnEdit.IsVisible = true;
        btnValider.IsVisible = false;
        btnDelete.IsVisible = false;
    }


    public void LoadPassword(ApiKey k)
    {
        Title = k.Name;
        tbxKey.TbxText = k.Key;
        tbxDescription.Text = k.Desctiption;
        tbxDate.Text = k.ExpirationDate.ToShortDateString();
    }


}