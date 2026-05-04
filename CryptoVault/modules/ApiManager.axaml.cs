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

public partial class ApiManager : UserControl
{
    private byte[] _key;
    private string _filePath;
    public ObservableCollection<ApiKey> ApiKeys { get; set; } = new ObservableCollection<ApiKey>();

    public ApiManager()
    {
        InitializeComponent();
    }

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

    private void SaveKeys()
    {
        string json = JsonSerializer.Serialize(ApiKeys);
        byte[] data = System.Text.Encoding.UTF8.GetBytes(json);
        byte[] encryptedData = CryptoService.Encrypt(data, _key);
        File.WriteAllBytes(_filePath, encryptedData);
    }

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

    private void BtnAdd_OnClick(object? sender, RoutedEventArgs e)
    {
        var newKey = new ApiKey { Name = "Nouvelle clé", Key = "", Desctiption = "", ExpirationDate = DateOnly.FromDateTime(DateTime.Now) };
        ApiKeys.Add(newKey);
        lbKeys.SelectedItem = newKey;
        keyInfo.SetEditMode(true);
        SaveKeys();
    }

    private void KeyInfo_OnKeyChanged(object? sender, EventArgs e)
    {
        if (lbKeys.SelectedItem is ApiKey selectedKey)
        {
            selectedKey.Name = keyInfo.Title ?? "Nouvelle clé";
            selectedKey.Key = keyInfo.tbxKey.TbxText ?? "";
            selectedKey.Desctiption = keyInfo.tbxDescription.Text ?? "";
            
            if (DateOnly.TryParse(keyInfo.tbxDate.Text, out DateOnly date))
            {
                selectedKey.ExpirationDate = date;
            }
            
            SaveKeys();
            
            // Refresh listbox by resetting ItemsSource since ApiKey doesn't implement INotifyPropertyChanged
            int idx = lbKeys.SelectedIndex;
            lbKeys.ItemsSource = null;
            lbKeys.ItemsSource = ApiKeys;
            lbKeys.SelectedIndex = idx;
        }
    }

    private void KeyInfo_OnKeyDeleted(object? sender, EventArgs e)
    {
        if (lbKeys.SelectedItem is ApiKey selectedKey)
        {
            ApiKeys.Remove(selectedKey);
            SaveKeys();
        }
    }
}