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
    /// <param name="fileName">The fileName to load the files from.</param>
    /// <returns>Stream containing the loaded file, null if no such file was found.</returns>
    public StoredFile GetFile(string fileName)
    {
        _logger.LogDebug("Getting {file} from filesystem", fileName);
        try
        {
            var path = Path.Join(_settings.BaseDirectory, fileName);
            var file = File.Open(path, FileMode.Open);
            return new StoredFile(path, file);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to open file {file}", fileName);
            return null;
        }
    }
}

/// <summary>
/// Settings for the <see cref="FileSystemFileSource"/>
/// </summary>
public class FileSystemFileSourceSettings : IFileSourceSettings
{
    /// <summary>
    /// The name of the file source to load.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The base directory files are stored.
    /// </summary>
    /// <remarks>When deploying as docker, this points to a directory in the docker container.</remarks>
    public string BaseDirectory { get; set; } = "/files";

    public bool Verify(ref List<string> errors)
    {
        if(string.IsNullOrWhiteSpace(BaseDirectory))
            errors.Add("No base directory given!");
        return !string.IsNullOrWhiteSpace(BaseDirectory);
    }
}