using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace CryptoVault.modules;

/// <summary>
/// User control for managing encrypted API keys.
/// </summary>
public partial class ApiManager : UserControl
{
    private byte[] _key;
    private string _filePath;

    /// <summary>
    /// Gets or sets the collection of API keys.
    /// </summary>
    public ObservableCollection<ApiKey> ApiKeys { get; set; } = new ObservableCollection<ApiKey>();

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiManager"/> class.
    /// </summary>
    public ApiManager()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiManager"/> class with a specific encryption key.
    /// </summary>
    /// <param name="key">The AES encryption key.</param>
    public ApiManager(byte[] key) : this()
    {
        _key = key;
        
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string vaultDir = Path.Combine(appData, "CryptoVault");
        Directory.CreateDirectory(vaultDir);
        _filePath = Path.Combine(vaultDir, "apikeys.enc");

        LoadKeys();
        
        lbKeys.ItemsSource = ApiKeys;
    }

    /// <summary>
    /// Loads and decrypts API keys from the storage file.
    /// </summary>
    private void LoadKeys()
    {
        if (File.Exists(_filePath))
        {
            try
            {
                byte[] encryptedData = File.ReadAllBytes(_filePath);
                byte[] decryptedData = CryptoService.Decrypt(encryptedData, _key);
                string json = System.Text.Encoding.UTF8.GetString(decryptedData);
                var keys = JsonSerializer.Deserialize<List<ApiKey>>(json);
                if (keys != null)
                {
                    ApiKeys = new ObservableCollection<ApiKey>(keys);
                }
            }
            catch
            {
                throw new Exception("Wrong password or corrupted file.");
            }
        }
    }

    /// <summary>
    /// Encrypts and saves API keys to the storage file.
    /// </summary>
    private void SaveKeys()
    {
        string json = JsonSerializer.Serialize(ApiKeys);
        byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
        byte[] encryptedData = CryptoService.Encrypt(data, _key);
        File.WriteAllBytes(_filePath, encryptedData);
    }

    /// <summary>
    /// Event handler for the selection change in the API keys listbox.
    /// </summary>
    private void LbKeys_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (lbKeys.SelectedItem is ApiKey selectedKey)
        {
            keyInfo.SetEditMode(false);
            keyInfo.LoadPassword(selectedKey);
            keyInfo.IsVisible = true;
        }
        else
        {
            keyInfo.IsVisible = false;
        }
    }

    /// <summary>
    /// Event handler for adding a new API key.
    /// </summary>
    private void BtnAdd_OnClick(object? sender, RoutedEventArgs e)
    {
        var newKey = new ApiKey { Name = "New key", Key = "", Desctiption = "", ExpirationDate = DateOnly.FromDateTime(DateTime.Now) }; // Translated from "Nouvelle clé"
        ApiKeys.Add(newKey);
        lbKeys.SelectedItem = newKey;
        keyInfo.SetEditMode(true);
        SaveKeys();
    }

    /// <summary>
    /// Event handler triggered when a key's information is changed and validated.
    /// </summary>
    private void KeyInfo_OnKeyChanged(object? sender, EventArgs e)
    {
        if (lbKeys.SelectedItem is ApiKey selectedKey)
        {
            selectedKey.Name = keyInfo.Title ?? "New key"; // Translated from "Nouvelle clé"
            selectedKey.Key = keyInfo.tbxKey.TbxText ?? "";
            selectedKey.Desctiption = keyInfo.tbxDescription.Text ?? "";
            
            if (keyInfo.dpDate.SelectedDate.HasValue)
            {
                selectedKey.ExpirationDate = DateOnly.FromDateTime(keyInfo.dpDate.SelectedDate.Value.Date);
            }
            
            SaveKeys();
            
            // Refresh listbox by resetting ItemsSource since ApiKey doesn't implement INotifyPropertyChanged
            int idx = lbKeys.SelectedIndex;
            lbKeys.ItemsSource = null;
            lbKeys.ItemsSource = ApiKeys;
            lbKeys.SelectedIndex = idx;
        }
    }

    /// <summary>
    /// Event handler triggered when an API key is deleted.
    /// </summary>
    private void KeyInfo_OnKeyDeleted(object? sender, EventArgs e)
    {
        if (lbKeys.SelectedItem is ApiKey selectedKey)
        {
            ApiKeys.Remove(selectedKey);
            SaveKeys();
        }
    }
}