using Azure.Identity;
using KingTech.Web.SimpleFileServer.Abstract.Models;
using KingTech.Web.SimpleFileServer.Abstract.Sources;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Drives.Item.Root;

namespace KingTech.Web.SimpleFileServer.OneDrivePlugin;

/// <summary>
/// File source to get files from Microsoft OneDrive.
/// This source uses the Microsoft Graph API (https://developer.microsoft.com/en-us/graph/graph-explorer)
/// See also: <seealso cref="IFileSource"/>
/// </summary>
public class OneDriveFileSource : IFileSource
{
    private const string rootItemKey = "root";

    private readonly ILogger<OneDriveFileSource> _logger;
    private readonly OneDriveFileSourceSettings _settings;

    /// <inheritdoc cref="IFileSource"/>
    public string Name => "OneDrive";

    /// <summary>
    /// The Graph Service Client that is used to get file from the Azure (OneDrive) API.
    /// </summary>
    private GraphServiceClient graphClient;

    /// <summary>
    /// File source to get files from Microsoft OneDrive.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="settings"></param>
    public OneDriveFileSource(ILogger<OneDriveFileSource> logger, OneDriveFileSourceSettings settings)
    {
        _logger = logger;
        _settings = settings;

        //There are multiple ways of identifying at the Microsoft Graph API, We will start with this but extend to more methods later.
        var credentials = new ClientSecretCredential(
            settings.TenantId,
            settings.ClientId,
            settings.ClientSecret,
            new ClientSecretCredentialOptions() { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud });


        graphClient = new GraphServiceClient(credentials, OneDriveFileSourceSettings.Scopes);
    }

    /// <summary>
    /// Load a file from OneDrive.
    /// See also <seealso cref="IFileSource"/>
    /// </summary>
    /// <param name="fileName">The name of the file to load.</param>
    /// <returns>Stream containing the loaded file, null if no such file was found.</returns>
    public async Task<StoredFile> GetFile(string fileName)
    {
        var itemId = GetDriveItemId(fileName);
        var result = await graphClient.Drives[_settings.DriveId].Items[fileName].Content.GetAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> ListFiles(string? directory)
    {
        var itemId = await GetDriveItemId(directory);

        var result = await graphClient.Drives[_settings.DriveId].Items[itemId].Children.GetAsync();
        if (result?.Value == null)
        {
            _logger.LogError("Failed to retrieve list of files for {item}.", itemId);
            return null; //TODO: Throw exceptions and catch them nicely in controller.
        }

        var files = result.Value.Where(r => r.Folder == null).ToList();
        
        return files.Select(f => f.Name);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> ListDirectories(string? directory)
    {
        var itemId = await GetDriveItemId(directory);

        var result = await graphClient.Drives[_settings.DriveId].Items[itemId].Children.GetAsync();
        if (result?.Value == null)
        {
            _logger.LogError("Failed to retrieve list of files for {item}.", itemId);
            return null; //TODO: Throw exceptions and catch them nicely in controller.
        }

        var folders = result.Value.Where(r => r.Folder != null).ToList();

        return folders.Select(f => f.Name);
    }

    /// <summary>
    /// Get the OneDrive ItemId for the given itemPath.
    /// If the 'UseItemId' setting is enabled, this will just return the itemPath.
    /// </summary>
    /// <param name="itemPath">The path to the item we want to fetch.</param>
    /// <returns>The OneDrive item ID corresponding to the file/folder in the given item path.</returns>
    private async Task<string?> GetDriveItemId(string? itemPath)
    {
        //Check if we are not using item ID's already.
        if (_settings.UseItemId)
            return itemPath ?? rootItemKey;

        //Get a list of all steps in the item path.
        var itemPathParts = new Uri(itemPath).Segments;
        var itemId = rootItemKey;

        //Loop throught the path to find the final item ID.
        foreach (var pathPart in itemPathParts)
        {
            var result = await graphClient.Drives[_settings.DriveId].Items[itemId].Children.GetAsync();
            if (result?.Value == null)
                throw new Exception("Failed to get item ID.");

            var item = result.Value.FirstOrDefault(r => r.Name.Equals(pathPart));
            if(item == null)
                throw new Exception("Failed to get item ID.");

            itemId = item.Id;
        }
        
        //Return the final itemId.
        return itemId;
    }
}