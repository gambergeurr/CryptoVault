using System.Collections.ObjectModel;

namespace CryptoVault;

public class FileSystemNode
{
    public string Name { get; set; } = "";
    public bool IsDirectory { get; set; }
    public string FullPath { get; set; } = "";
    public FileSystemNode? Parent { get; set; }
    public ObservableCollection<FileSystemNode> Children { get; set; } = new ObservableCollection<FileSystemNode>();
    public string Icon => IsDirectory ? "📁" : "📄";
}