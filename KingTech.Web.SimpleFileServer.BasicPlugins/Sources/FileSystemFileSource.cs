using KingTech.Web.SimpleFileServer.Abstract.Models;
using KingTech.Web.SimpleFileServer.Abstract.Sources;
using Microsoft.Extensions.Logging;

namespace KingTech.Web.SimpleFileServer.BasicPlugins.Sources;

/// <summary>
/// File source to get files from the file system.
/// See also: <seealso cref="IFileSource"/>
/// </summary>
public class FileSystemFileSource : IFileSource
{
    /// <inheritdoc cref="IFileSource"/>
    public string Name => "FileSystem";

    /// <inheritdoc cref="IFileSource"/>
    public bool Enabled => _settings?.Enabled ?? false;

    private readonly ILogger<FileSystemFileSource> _logger;
    private readonly FileSystemFileSourceSettings _settings;

    /// <summary>
    /// File source to get files from the file system.
    /// </summary>
    /// <param name="logger">Logger to log errors to. See also: <seealso cref="ILogger{TCategoryName}"/></param>
    /// <param name="settings">Settings for the file system logging. See also: <seealso cref="IFileSourceSettings"/></param>
    public FileSystemFileSource(ILogger<FileSystemFileSource> logger, FileSystemFileSourceSettings settings)
    {
        _logger = logger;
        _settings = settings;
    }

    /// <summary>
    /// Load a file from the file system.
    /// See also <seealso cref="IFileSource"/>
    /// </summary>
    /// <param name="fileName">The name of the file to load.</param>
    /// <returns>Stream containing the loaded file, null if no such file was found.</returns>
    public Task<StoredFile> GetFile(string fileName)
    {
        _logger.LogDebug("Getting {file} from filesystem", fileName);
        try
        {
            //Combine with base directory.
            var path = Path.Join(_settings.BaseDirectory, fileName);

            //Try to open file.
            var file = File.Open(path, FileMode.Open);
            return Task.FromResult(new StoredFile(path, file));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to open file {file}", fileName);
            return Task.FromResult<StoredFile>(null);
        }
    }

    /// <inheritdoc/>
    public Task<IEnumerable<string>> ListFiles(string? directory)
    {
        //Combine with base directory.
        var path = Path.Join(_settings.BaseDirectory, string.IsNullOrWhiteSpace(directory) ? string.Empty : directory);

        var fullFilePaths = Directory.GetFiles(path).ToList();
        return Task.FromResult(fullFilePaths.Select(fp => Path.GetFileName(fp)));
    }

    /// <inheritdoc/>
    public Task<IEnumerable<string>> ListDirectories(string? directory)
    {
        //Combine with base directory.
        var path = Path.Join(_settings.BaseDirectory, string.IsNullOrWhiteSpace(directory) ? string.Empty : directory);

        //return Directory.GetDirectories(path);

        var fullDirectoryPaths = Directory.GetDirectories(path);
        return Task.FromResult(fullDirectoryPaths.Select(fp => Path.GetFileName(fp)));
    }
}