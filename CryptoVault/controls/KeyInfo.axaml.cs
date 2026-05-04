using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace CryptoVault.controls;

public partial class KeyInfo : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<KeyInfo, string>(nameof(Title), defaultValue: string.Empty);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<bool> DisplayModeProperty =
        AvaloniaProperty.Register<KeyInfo, bool>(nameof(DisplayMode), defaultValue: true);

    public bool DisplayMode
    {
        get => GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }
    public event EventHandler? KeyChanged;
    public event EventHandler? KeyDeleted;


    public KeyInfo()
    {
        InitializeComponent();
        DisplayMode = true;
    }
    
    public void SetEditMode(bool isEditing)
    {
        DisplayMode = !isEditing;
        btnEdit.IsVisible = !isEditing;
        btnValider.IsVisible = isEditing;
        btnDelete.IsVisible = isEditing;
    }

    private void BtnEdit_OnClick(object? sender, RoutedEventArgs e)
    {
        SetEditMode(true);
    }
    
    private void BtnValider_OnClick(object? sender, RoutedEventArgs e)
    {
        SetEditMode(false);
        KeyChanged?.Invoke(this, EventArgs.Empty);
    }
    
    private void BtnDelete_OnClick(object? sender, RoutedEventArgs e)
    {
        tbxTitle.Text = null;
        tbxKey.TbxText = null;
        tbxDescription.Text = null;
        tbxDate.Text = null;
        
        SetEditMode(false);
        KeyDeleted?.Invoke(this, EventArgs.Empty);
    }


    public void LoadPassword(ApiKey k)
    {
        Title = k.Name;
        tbxKey.TbxText = k.Key;
        tbxDescription.Text = k.Desctiption;
        tbxDate.Text = k.ExpirationDate.ToShortDateString();
    }


}