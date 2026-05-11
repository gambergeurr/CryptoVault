using System.Collections.ObjectModel;

namespace CryptoVault;

/// <summary>
/// Represents a node in the virtual file system hierarchy.
/// </summary>
public class FileSystemNode
{
    /// <summary>
    /// Gets or sets the name of the file or directory.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Gets or sets a value indicating whether this node represents a directory.
    /// </summary>
    public bool IsDirectory { get; set; }

    /// <summary>
    /// Gets or sets the full path of the node within the virtual file system.
    /// </summary>
    public string FullPath { get; set; } = "";

    /// <summary>
    /// Gets or sets the parent node of this node. Null if it is a root node.
    /// </summary>
    public FileSystemNode? Parent { get; set; }

    /// <summary>
    /// Gets or sets the collection of child nodes.
    /// </summary>
    public ObservableCollection<FileSystemNode> Children { get; set; } = new ObservableCollection<FileSystemNode>();

    /// <summary>
    /// Gets the icon representing the node type (directory or file).
    /// </summary>
    public string Icon => IsDirectory ? "📁" : "📄";
}