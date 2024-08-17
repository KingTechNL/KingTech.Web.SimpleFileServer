namespace KingTech.Web.SimpleFileServer.OneDrivePlugin.Clients;

/// <summary>
/// Interface for client implementations that allow us to access files on Microsoft OneDrive.
/// </summary>
public interface IOneDriveClient
{
    public static string RootItemKey = "root";

    /// <summary>
    /// Get the file stream for the given file name.
    /// </summary>
    /// <param name="fileName">The file name to get a file steam for.</param>
    /// <returns>The file stream for the given OneDrive item, null if no such file could be found.</returns>
    public Task<Stream> GetFileStream(string fileName);

    /// <summary>
    /// List all the file names and ID's in a given directory.
    /// </summary>
    /// <param name="directory">The directory (ItemId) to look in.</param>
    /// <returns>A list of all the file names and ID's in a given directory.</returns>
    public Task<IEnumerable<(string Name, string ItemId)>> ListFiles(string directory);

    /// <summary>
    /// List all the directory names and ID's in a given directory.
    /// </summary>
    /// <param name="directory">The directory (ItemId) to look in.</param>
    /// <returns>A list of all the directory names and ID's in a given directory.</returns>
    public Task<IEnumerable<(string Name, string ItemId)>?> ListDirectories(string directory);
}