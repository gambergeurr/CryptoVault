using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace CryptoVault.controls;

/// <summary>
/// Represents a custom TextBox control with an integrated copy-to-clipboard button.
/// </summary>
public partial class CopyableTextbox : UserControl
{
    /// <summary>
    /// Defines the <see cref="PasswordChar"/> property.
    /// </summary>
    public static readonly StyledProperty<char> PasswordCharProperty =
        AvaloniaProperty.Register<CopyableTextbox, char>(nameof(PasswordChar), defaultValue: '\0');

    /// <summary>
    /// Gets or sets the password character used to obscure text.
    /// </summary>
    public char PasswordChar
    {
        get => GetValue(PasswordCharProperty);
        set => SetValue(PasswordCharProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="TbxText"/> property.
    /// </summary>
    public static readonly StyledProperty<string> TbxTextProperty =
        AvaloniaProperty.Register<CopyableTextbox, string>(nameof(TbxText), defaultValue: string.Empty);

    /// <summary>
    /// Gets or sets the text content of the textbox.
    /// </summary>
    public string TbxText
    {
        get => GetValue(TbxTextProperty);
        set => SetValue(TbxTextProperty, value);
    }
    
    /// <summary>
    /// Defines the <see cref="IsReadOnly"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<CopyableTextbox, bool>(
            nameof(IsReadOnly), 
            defaultValue: true);
    
    /// <summary>
    /// Gets or sets a value indicating whether the textbox is read-only.
    /// </summary>
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show a reveal password button.
    /// Dynamically updates CSS classes.
    /// </summary>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="CopyableTextbox"/> class.
    /// </summary>
    public CopyableTextbox()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Event handler for the copy button click. Copies the current text to the clipboard.
    /// </summary>
    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if(!string.IsNullOrEmpty(tbxContent.Text))
            TopLevel.GetTopLevel(this).Clipboard.SetTextAsync(tbxContent.Text);
    }
}