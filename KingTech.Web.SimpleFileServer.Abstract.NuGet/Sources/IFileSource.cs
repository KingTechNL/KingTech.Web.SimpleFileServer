using KingTech.Web.SimpleFileServer.Abstract.Models;

namespace KingTech.Web.SimpleFileServer.Abstract.Sources;

/// <summary>
/// File sources are used to fetch files from different locations, e.g. the filesystem, an sFTP server or cloud service.
/// </summary>
public interface IFileSource
{
    /// <summary>
    /// The unique name of this file source. Used to determine whether the source should be loaded / used or not.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Load a file from the given fileName and return it as a stream.
    /// </summary>
    /// <param name="fileName">The fileName to get the file from.</param>
    /// <returns>StoredFile with metadata and stream containing the requested file, null when no such file could be found.</returns>
    public StoredFile GetFile(string fileName);

    /// <summary>
    /// List all the files available to this file source.
    /// </summary>
    /// <param name="directory">The directory to list files in.</param>
    /// <returns>A list of all file names in the given directory.</returns>
    public IEnumerable<string> ListFiles(string? directory);

    /// <summary>
    /// List all the (sub) directories available to this file source.
    /// </summary>
    /// <param name="directory">The directory to sub-directories in.</param>
    /// <returns>A list of all (sub) directory names in the given directory.</returns>
    public IEnumerable<string> ListDirectories(string? directory);
}