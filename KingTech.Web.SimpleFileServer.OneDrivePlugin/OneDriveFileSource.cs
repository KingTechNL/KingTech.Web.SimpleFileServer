using KingTech.Web.SimpleFileServer.Abstract.Models;
using KingTech.Web.SimpleFileServer.Abstract.Sources;
using KingTech.Web.SimpleFileServer.OneDrivePlugin.Clients;
using Microsoft.Extensions.Logging;

namespace KingTech.Web.SimpleFileServer.OneDrivePlugin;

/// <summary>
/// File source to get files from Microsoft OneDrive.
/// See also: <seealso cref="IFileSource"/>
/// </summary>
public class OneDriveFileSource : FileSourceBase<OneDriveFileSourceSettings>
{
    private readonly ILogger<OneDriveFileSource> _logger;
    
    /// <summary>
    /// Client handling communication with OneDrive.
    /// </summary>
    private IOneDriveClient _client;

    /// <summary>
    /// File source to get files from Microsoft OneDrive.
    /// </summary>
    /// <param name="loggerFactory"></param>
    /// <param name="settings"></param>
    public OneDriveFileSource(ILoggerFactory loggerFactory, OneDriveFileSourceSettings settings) : base(settings)
    {
        _logger = loggerFactory.CreateLogger<OneDriveFileSource>();

        //Dont start a connection if this plugin is disabled.
        if (!settings.Enabled)
            return;

        if (settings.ClientSecretCredentials != null)
        {
            _client = new MicrosoftGraphApiClient(loggerFactory.CreateLogger<MicrosoftGraphApiClient>(), settings);
        }else if (settings.AuthorizationCodeCredentials != null)
        {
            _client = new OneDriveApiClient(loggerFactory.CreateLogger<OneDriveApiClient>(), settings);
        }
        else
        {
            _logger.LogError("No valid OneDrive credentials given.");
        }
    }

    /// <summary>
    /// Load a file from OneDrive.
    /// See also <seealso cref="IFileSource"/>
    /// </summary>
    /// <param name="fileName">The name of the file to load.</param>
    /// <returns>Stream containing the loaded file, null if no such file was found.</returns>
    public override async Task<StoredFile> GetFile(string fileName)
    {
        var result = await _client.GetFileStream(fileName);
        return new StoredFile(fileName, result);
    }

    /// <inheritdoc/>
    public override async Task<IEnumerable<string>> ListFiles(string? directory)
    {
        var files = await _client.ListFiles(directory);
        return files?.Select(f => f.Name);
    }

    /// <inheritdoc/>
    public override async Task<IEnumerable<string>> ListDirectories(string? directory)
    {
        var folders = await _client.ListDirectories(directory);
        return folders?.Select(f => f.Name);
    }
}