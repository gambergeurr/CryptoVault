using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace CryptoVault.controls;

public partial class CopyableTextbox : UserControl
{
    public static readonly StyledProperty<char> PasswordCharProperty =
        AvaloniaProperty.Register<CopyableTextbox, char>(nameof(PasswordChar), defaultValue: '\0');

    public char PasswordChar
    {
        get => GetValue(PasswordCharProperty);
        set => SetValue(PasswordCharProperty, value);
    }

    public static readonly StyledProperty<string> TbxTextProperty =
        AvaloniaProperty.Register<CopyableTextbox, string>(nameof(TbxText), defaultValue: string.Empty);

    public string TbxText
    {
        get => GetValue(TbxTextProperty);
        set => SetValue(TbxTextProperty, value);
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
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if(!string.IsNullOrEmpty(tbxContent.Text))
            TopLevel.GetTopLevel(this).Clipboard.SetTextAsync(tbxContent.Text);
    }
}