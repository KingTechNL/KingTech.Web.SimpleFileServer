namespace KingTech.Web.SimpleFileServer.GitHubPlugins.Clients;

/// <summary>
/// Client used for accessing GitHub repositories as file source.
/// </summary>
public interface IGitHubClient
{

    /// <summary>
    /// Get a specific item from GitHub.
    /// </summary>
    /// <param name="itemPath">The path of the item on GitHub.</param>
    /// <returns>A <see cref="GitItem"/> describing the retrieved item, null if the item did not exist.</returns>
    public Task<Stream> GetFile(string itemPath);

    /// <summary>
    /// Get a list of all files in the given GitHub directory.
    /// </summary>
    /// <param name="directory">The directory  to look in.</param>
    /// <returns>A list of all the files in a given directory, null if the directory did not exist.</returns>
    public Task<IEnumerable<string>> GetFiles(string? directory);

    /// <summary>
    /// Get a list of all directories in the given GitHub directory.
    /// </summary>
    /// <param name="directory">The directory  to look in.</param>
    /// <returns>A list of all the directories in a given directory, null if the directory did not exist.</returns>
    public Task<IEnumerable<string>> GetDirectories(string? directory);
}