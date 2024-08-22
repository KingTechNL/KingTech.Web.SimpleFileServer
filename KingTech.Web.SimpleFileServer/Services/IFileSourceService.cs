using KingTech.Web.SimpleFileServer.Abstract.Models;
using KingTech.Web.SimpleFileServer.Abstract.Sources;

namespace KingTech.Web.SimpleFileServer.Services;

/// <summary>
/// This service is responsible for interacting with the registered FileSource plugins.
/// </summary>
public interface IFileSourceService
{
    /// <summary>
    /// Get a list of file names in the given directory.
    /// </summary>
    /// <param name="directory">The directory to look in, if no directory is given we use the root dir.</param>
    /// <returns>A list of file names in the given directory.</returns>
    Task<IEnumerable<string>> GetFiles(string? directory);

    /// <summary>
    /// Get a list of directory names in the given directory.
    /// </summary>
    /// <param name="directory">The directory to look in, if no directory is given we use the root dir.</param>
    /// <returns>A list of directory names in the given directory.</returns>
    Task<IEnumerable<string>> GetDirectories(string? directory);

    /// <summary>
    /// Load the file using the registered <see cref="IFileSource"/>s.
    /// </summary>
    /// <param name="fileName">File path/name.</param>
    /// <returns>The loaded file including metadata, or null if no such file was found.</returns>
    Task<StoredFile> GetFile(string fileName);
}