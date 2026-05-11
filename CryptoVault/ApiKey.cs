using System;
using Avalonia.Platform.Storage;

namespace CryptoVault;

/// <summary>
/// Represents an API Key entity stored securely.
/// </summary>
public class ApiKey
{
    /// <summary>
    /// Gets or sets the name of the API key.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the actual API key string.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets the description for the API key.
    /// </summary>
    public string Desctiption { get; set; }

    /// <summary>
    /// Gets or sets the expiration date of the API key.
    /// </summary>
    public DateOnly ExpirationDate { get; set; }
}