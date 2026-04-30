using System;
using Avalonia.Platform.Storage;

namespace CryptoVault;

public class ApiKey
{
    public string Name { get; set; }
    public string Key { get; set; }
    public string Desctiption { get; set; }
    public DateOnly ExpirationDate { get; set; }
}