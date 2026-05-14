using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace CryptoVault.controls;

/// <summary>
/// A user control for displaying and editing API key information.
/// </summary>
public partial class KeyInfo : UserControl
{
    /// <summary>
    /// Defines the <see cref="Title"/> property.
    /// </summary>
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<KeyInfo, string>(nameof(Title), defaultValue: string.Empty);

    /// <summary>
    /// Gets or sets the title of the key.
    /// </summary>
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="DisplayMode"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> DisplayModeProperty =
        AvaloniaProperty.Register<KeyInfo, bool>(nameof(DisplayMode), defaultValue: true);

    /// <summary>
    /// Gets or sets a value indicating whether the control is in display mode (true) or edit mode (false).
    /// </summary>
    public bool DisplayMode
    {
        get => GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    /// <summary>
    /// Occurs when the key information has been changed and validated.
    /// </summary>
    public event EventHandler? KeyChanged;

    /// <summary>
    /// Occurs when the key has been deleted.
    /// </summary>
    public event EventHandler? KeyDeleted;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyInfo"/> class.
    /// </summary>
    public KeyInfo()
    {
        InitializeComponent();
        DisplayMode = true;
    }
    
    /// <summary>
    /// Toggles the user interface between edit and display modes.
    /// </summary>
    /// <param name="isEditing">True to enable edit mode, false for display mode.</param>
    public void SetEditMode(bool isEditing)
    {
        DisplayMode = !isEditing;
        btnEdit.IsVisible = !isEditing;
        btnValider.IsVisible = isEditing;
        btnDelete.IsVisible = isEditing;
    }

    /// <summary>
    /// Event handler for the edit button click. Enables edit mode.
    /// </summary>
    private void BtnEdit_OnClick(object? sender, RoutedEventArgs e)
    {
        SetEditMode(true);
    }
    
    /// <summary>
    /// Event handler for the validate button click. Saves changes and returns to display mode.
    /// </summary>
    private void BtnValider_OnClick(object? sender, RoutedEventArgs e)
    {
        SetEditMode(false);
        KeyChanged?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// Event handler for the delete button click. Clears all fields and triggers the KeyDeleted event.
    /// </summary>
    private void BtnDelete_OnClick(object? sender, RoutedEventArgs e)
    {
        tbxTitle.Text = null;
        tbxKey.TbxText = null;
        tbxDescription.Text = null;
        dpDate.SelectedDate = null;
        txtDateDisplay.Text = null;
        txtRemainingDays.Text = null;
        
        SetEditMode(false);
        KeyDeleted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Loads an <see cref="ApiKey"/> instance into the control for display.
    /// </summary>
    /// <param name="k">The API key to display.</param>
    public void LoadPassword(ApiKey k)
    {
        Title = k.Name;
        tbxKey.TbxText = k.Key;
        tbxDescription.Text = k.Desctiption;
        dpDate.SelectedDate = k.ExpirationDate.ToDateTime(TimeOnly.MinValue);
        txtDateDisplay.Text = k.ExpirationDate.ToShortDateString();
        UpdateRemainingDaysDisplay(k.ExpirationDate);
    }

    /// <summary>
    /// Updates the text and color of the remaining days display based on the expiration date.
    /// </summary>
    /// <param name="expirationDate">The expiration date.</param>
    private void UpdateRemainingDaysDisplay(DateOnly expirationDate)
    {
        int daysRemaining = (expirationDate.ToDateTime(TimeOnly.MinValue) - DateTime.Now.Date).Days;
        if (daysRemaining > 0)
        {
            txtRemainingDays.Text = $"({daysRemaining} jours restants)";
            txtRemainingDays.Foreground = Avalonia.Media.Brushes.Gray;
        }
        else if (daysRemaining == 0)
        {
            txtRemainingDays.Text = "(Expire aujourd'hui)";
            txtRemainingDays.Foreground = Avalonia.Media.Brushes.Orange;
        }
        else
        {
            txtRemainingDays.Text = $"(Expiré depuis {-daysRemaining} jours)";
            txtRemainingDays.Foreground = Avalonia.Media.Brushes.Red;
        }
    }
}