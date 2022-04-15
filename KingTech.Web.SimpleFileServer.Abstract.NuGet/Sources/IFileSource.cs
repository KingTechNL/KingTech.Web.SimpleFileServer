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
}