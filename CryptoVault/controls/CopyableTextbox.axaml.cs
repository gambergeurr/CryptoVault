using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace CryptoVault.controls;

public partial class CopyableTextbox : UserControl
{
    public char PasswordChar
    {
        get;
        set
        {
            field = value;
            tbxContent.PasswordChar = value;
        }
    }

    public string TbxText
    {
        get;
        set
        {
            field = value;
            tbxContent.Text = value;
        }
    }
    
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<CopyableTextbox, bool>(
            nameof(IsReadOnly), 
            defaultValue: true);
    
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public bool HasPasswordButton
    {
        get;
        set
        {
            field = value;
            if (value)
            {
                tbxContent.Classes.Add("revealPasswordButton");
            }
            else
            {
                tbxContent.Classes.Remove("revealPasswordButton");
            }
        }
    }

    public CopyableTextbox()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if(!string.IsNullOrEmpty(tbxContent.Text))
            TopLevel.GetTopLevel(this).Clipboard.SetTextAsync(tbxContent.Text);
    }
}