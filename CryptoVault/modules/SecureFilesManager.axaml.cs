using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Input;

namespace CryptoVault.modules;

/// <summary>
/// User control for managing a secure, encrypted file vault.
/// </summary>
public partial class SecureFilesManager : UserControl
{
    private byte[] key;
    private string vaultPath;

    /// <summary>
    /// Gets or sets the collection of root nodes in the virtual file system.
    /// </summary>
    public ObservableCollection<FileSystemNode> RootNodes { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of nodes currently being displayed.
    /// </summary>
    public ObservableCollection<FileSystemNode> CurrentDisplayNodes { get; set; } = new();
    
    private FileSystemNode? currentNode;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecureFilesManager"/> class.
    /// </summary>
    public SecureFilesManager()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecureFilesManager"/> class with a specific encryption key.
    /// </summary>
    /// <param name="key">The AES encryption key.</param>
    public SecureFilesManager(byte[] key) : this()
    {
        this.key = key;
        
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string vaultDir = Path.Combine(appData, "CryptoVault");
        Directory.CreateDirectory(vaultDir);
        vaultPath = Path.Combine(vaultDir, "securefiles.enc");

        LoadFileSystem();
        NavigateToNode(null); // root directory
    }

    /// <summary>
    /// Navigates to a specific node in the file system, updating the display.
    /// </summary>
    /// <param name="node">The target node to navigate to, or null for the root directory.</param>
    private void NavigateToNode(FileSystemNode? node)
    {
        currentNode = node;
        CurrentDisplayNodes.Clear();
        
        var items = node == null ? RootNodes : node.Children;
        foreach(var item in items)
        {
            CurrentDisplayNodes.Add(item);
        }

        txtCurrentPath.Text = node == null ? "/" : "/" + node.FullPath;
        btnGoUp.IsEnabled = node != null;
        lbFiles.ItemsSource = CurrentDisplayNodes;
    }

    /// <summary>
    /// Event handler for navigating up one directory level.
    /// </summary>
    private void BtnGoUp_OnClick(object? sender, RoutedEventArgs e)
    {
        if (currentNode != null)
        {
            NavigateToNode(currentNode.Parent);
        }
    }

    /// <summary>
    /// Event handler for double-tapping an item in the file list. Navigates into directories.
    /// </summary>
    private void LbFiles_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (lbFiles.SelectedItem is FileSystemNode selectedNode)
        {
            if (selectedNode.IsDirectory)
            {
                NavigateToNode(selectedNode);
            }
        }
    }

    /// <summary>
    /// Loads and parses the encrypted file system archive into the virtual node structure.
    /// </summary>
    private void LoadFileSystem()
    {
        RootNodes.Clear();
        if (!File.Exists(vaultPath)) return;

        try
        {
            byte[] encryptedData = File.ReadAllBytes(vaultPath);
            byte[] decryptedData = CryptoService.Decrypt(encryptedData, key);
            
            using var ms = new MemoryStream(decryptedData);
            using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
            
            foreach (var entry in archive.Entries)
            {
                AddNode(entry.FullName);
            }
        }
        catch
        {
            throw new Exception("Wrong password or corrupted secure files vault.");
        }
    }

    /// <summary>
    /// Parses a file path from the zip archive and adds it to the virtual node structure.
    /// </summary>
    /// <param name="path">The full path of the entry within the archive.</param>
    private void AddNode(string path)
    {
        string[] parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;

        var currentCollection = RootNodes;
        string currentPath = "";
        FileSystemNode? currentParent = null;

        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i];
            bool isLast = (i == parts.Length - 1);
            bool isDir = !isLast || path.EndsWith("/");
            
            currentPath += part + (isDir ? "/" : "");

            var existingNode = FindNode(currentCollection, part);
            if (existingNode == null)
            {
                existingNode = new FileSystemNode
                {
                    Name = part,
                    IsDirectory = isDir,
                    FullPath = currentPath,
                    Parent = currentParent
                };
                currentCollection.Add(existingNode);
            }
            currentCollection = existingNode.Children;
            currentParent = existingNode;
        }
    }

    /// <summary>
    /// Finds a node by name within a collection.
    /// </summary>
    /// <param name="collection">The collection to search within.</param>
    /// <param name="name">The name of the node to find.</param>
    /// <returns>The found node, or null if not found.</returns>
    private FileSystemNode? FindNode(ObservableCollection<FileSystemNode> collection, string name)
    {
        foreach (var node in collection)
        {
            if (node.Name == name) return node;
        }
        return null;
    }

    /// <summary>
    /// Performs an update operation on the encrypted archive.
    /// </summary>
    /// <param name="updateAction">An action defining the zip archive modifications.</param>
    private void SaveFileSystem(Action<ZipArchive> updateAction)
    {
        byte[] decryptedData;
        if (File.Exists(vaultPath))
        {
            byte[] encryptedData = File.ReadAllBytes(vaultPath);
            decryptedData = CryptoService.Decrypt(encryptedData, key);
        }
        else
        {
            // Empty valid zip. 22 bytes.
            decryptedData = new byte[] { 80, 75, 5, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        }

        using (var ms = new MemoryStream())
        {
            ms.Write(decryptedData, 0, decryptedData.Length);
            ms.Position = 0;

            using (var archive = new ZipArchive(ms, ZipArchiveMode.Update, true))
            {
                updateAction(archive);
            }

            decryptedData = ms.ToArray();
        }

        byte[] newEncryptedData = CryptoService.Encrypt(decryptedData, key);
        File.WriteAllBytes(vaultPath, newEncryptedData);

        LoadFileSystem();
        NavigateToNode(currentNode == null ? null : ReFindNode(currentNode.FullPath));
    }

    /// <summary>
    /// Refinds a node by its full path after a file system reload.
    /// </summary>
    /// <param name="fullPath">The full path of the node to find.</param>
    /// <returns>The found node, or null if not found.</returns>
    private FileSystemNode? ReFindNode(string fullPath)
    {
        string[] parts = fullPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var currentCollection = RootNodes;
        FileSystemNode? target = null;
        
        foreach (var part in parts)
        {
            target = FindNode(currentCollection, part);
            if (target == null) return null;
            currentCollection = target.Children;
        }
        return target;
    }

    /// <summary>
    /// Event handler for adding files to the secure vault.
    /// </summary>
    private async void BtnAddFile_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select a file to secure", // Translated from "Sélectionner un fichier à sécuriser"
            AllowMultiple = true
        });

        if (files.Count > 0)
        {
            SaveFileSystem(archive =>
            {
                foreach (var file in files)
                {
                    string basePath = currentNode != null ? currentNode.FullPath : "";

                    string entryName = basePath + file.Name;
                    
                    var existing = archive.GetEntry(entryName);
                    existing?.Delete();

                    if (file.TryGetLocalPath() is string localPath)
                    {
                        archive.CreateEntryFromFile(localPath, entryName);
                    }
                }
            });
        }
    }

    /// <summary>
    /// Event handler for deleting a selected file or folder from the secure vault.
    /// </summary>
    private void BtnDelete_OnClick(object? sender, RoutedEventArgs e)
    {
        if (lbFiles.SelectedItem is not FileSystemNode selectedNode) return;

        SaveFileSystem(archive =>
        {
            if (selectedNode.IsDirectory)
            {
                var entriesToDelete = archive.Entries
                    .Where(entry => entry.FullName.StartsWith(selectedNode.FullPath))
                    .ToList();
                    
                foreach (var entry in entriesToDelete)
                {
                    entry.Delete();
                }
            }
            else
            {
                var entry = archive.GetEntry(selectedNode.FullPath);
                entry?.Delete();
            }
        });
    }

    /// <summary>
    /// Event handler for importing an entire folder into the secure vault.
    /// </summary>
    private async void BtnImportFolder_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select a folder to import", // Translated from "Sélectionner un dossier à importer"
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            var folder = folders[0];
            if (folder.TryGetLocalPath() is string localFolderPath)
            {
                string folderName = new DirectoryInfo(localFolderPath).Name;
                string basePath = currentNode != null ? currentNode.FullPath : "";
                string baseEntryPath = basePath + folderName + "/";

                SaveFileSystem(archive =>
                {
                    if (archive.GetEntry(baseEntryPath) == null)
                    {
                        archive.CreateEntry(baseEntryPath);
                    }

                    void AddDirectoryToArchive(string currentLocalPath, string currentArchivePath)
                    {
                        foreach (string filePath in Directory.GetFiles(currentLocalPath))
                        {
                            string fileName = Path.GetFileName(filePath);
                            string entryName = currentArchivePath + fileName;
                            
                            var existing = archive.GetEntry(entryName);
                            existing?.Delete();

                            archive.CreateEntryFromFile(filePath, entryName);
                        }

                        foreach (string dirPath in Directory.GetDirectories(currentLocalPath))
                        {
                            string dirName = Path.GetFileName(dirPath);
                            string newArchivePath = currentArchivePath + dirName + "/";
                            
                            if (archive.GetEntry(newArchivePath) == null)
                            {
                                archive.CreateEntry(newArchivePath);
                            }

                            AddDirectoryToArchive(dirPath, newArchivePath);
                        }
                    }

                    AddDirectoryToArchive(localFolderPath, baseEntryPath);
                });
            }
        }
    }

    /// <summary>
    /// Event handler for extracting the selected file or folder out of the secure vault.
    /// </summary>
    private async void BtnExtract_OnClick(object? sender, RoutedEventArgs e)
    {
        if (lbFiles.SelectedItem is not FileSystemNode selectedNode) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var destFolders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select a destination folder for extraction", // Translated from "Sélectionner un dossier de destination pour l'extraction"
            AllowMultiple = false
        });

        if (destFolders.Count == 0) return;
        
        var destFolder = destFolders[0];
        if (destFolder.TryGetLocalPath() is not string destFolderPath) return;

        try
        {
            byte[] encryptedData = File.ReadAllBytes(vaultPath);
            byte[] decryptedData = CryptoService.Decrypt(encryptedData, key);
            
            using var ms = new MemoryStream(decryptedData);
            using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
            
            foreach (var entry in archive.Entries)
            {
                if (entry.FullName.StartsWith(selectedNode.FullPath))
                {
                    string relativePath = entry.FullName.Substring(selectedNode.FullPath.Length);
                    string extractPath;
                    
                    if (selectedNode.IsDirectory)
                    {
                         extractPath = Path.Combine(destFolderPath, selectedNode.Name, relativePath.Replace('/', Path.DirectorySeparatorChar));
                    }
                    else
                    {
                         extractPath = Path.Combine(destFolderPath, selectedNode.Name);
                    }

                    if (entry.FullName.EndsWith("/"))
                    {
                        Directory.CreateDirectory(extractPath);
                    }
                    else
                    {
                        var dir = Path.GetDirectoryName(extractPath);
                        if (dir != null) Directory.CreateDirectory(dir);
                        entry.ExtractToFile(extractPath, overwrite: true);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Error handling could be implemented here
        }
    }

    /// <summary>
    /// Event handler for checking the integrity of the selected file.
    /// </summary>
    private void BtnCheckIntegrity_OnClick(object? sender, RoutedEventArgs e)
    {
        FileSystemNode selectedNode = lbFiles.SelectedItem as FileSystemNode;
        if (selectedNode == null || selectedNode.IsDirectory)
        {
            // Only files can be checked for integrity
            return;
        }

        try
        {
            byte[] encryptedData = File.ReadAllBytes(vaultPath);
            byte[] decryptedData = CryptoService.Decrypt(encryptedData, key);
            
            using var ms = new MemoryStream(decryptedData);
            using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
            
            var entry = archive.GetEntry(selectedNode.FullPath);
            if (entry == null) return;

            using var entryStream = entry.Open();
            using var entryMs = new MemoryStream();
            entryStream.CopyTo(entryMs);
            byte[] fileData = entryMs.ToArray();

            string vaultHash = CryptoService.Hash(fileData);

            var integrityWindow = new IntegrityCheckWindow(selectedNode.Name, vaultHash);
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is Window mainWindow)
            {
                integrityWindow.ShowDialog(mainWindow);
            }
        }
        catch (Exception ex)
        {
            // Error handling could be implemented here
        }
    }
}