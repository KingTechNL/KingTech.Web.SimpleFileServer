using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace KingTech.Web.SimpleFileServer.OneDrivePlugin.Clients;

/// <summary>
/// <see cref="IOneDriveClient"/> that used Microsofts Graph API to access OneDrive (https://developer.microsoft.com/en-us/graph/graph-explorer).
/// This client uses ClientSecretCredential authentication which is only available to OneDrive Business users.
/// </summary>
public class MicrosoftGraphApiClient : IOneDriveClient
{
    private static readonly string[] Scopes = { "https://graph.microsoft.com/.default" };

    private readonly ILogger<MicrosoftGraphApiClient> _logger;
    private readonly OneDriveFileSourceSettings _settings;

    /// <summary>
    /// The Graph Service Client that is used to get file from the Azure (OneDrive) API.
    /// </summary>
    private readonly GraphServiceClient _client;

    public MicrosoftGraphApiClient(ILogger<MicrosoftGraphApiClient> logger, OneDriveFileSourceSettings settings)
    {
        _logger = logger;
        _settings = settings;

        //There are multiple ways of identifying at the Microsoft Graph API, We will start with this but extend to more methods later.
        var credentials = new ClientSecretCredential(
            settings.ClientSecretCredentials?.TenantId,
            settings.ClientSecretCredentials?.ClientId,
            settings.ClientSecretCredentials?.ClientSecret,
            new ClientSecretCredentialOptions() { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud });

        _client = new GraphServiceClient(credentials, Scopes);
    }


    /// <inheritdoc />
    public async Task<Stream> GetFileStream(string fileName)
    {
        var itemId = await GetDriveItemId(fileName);
        var result = await _client.Drives[_settings.DriveId].Items[itemId].Content.GetAsync();
        
        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<(string Name, string ItemId)>?> ListFiles(string directory)
    {
        var itemId = await GetDriveItemId(directory);

        var result = await _client.Drives[_settings.DriveId].Items[itemId].Children.GetAsync();
        if (result?.Value == null)
        {
            _logger.LogError("Failed to retrieve list of files for {item}.", itemId);
            return null; //TODO: Throw exceptions and catch them nicely in controller.
        }
        
        var files = result.Value.Where(r => r.Folder == null).ToList();

        return files.Select(f => (f.Name, f.Id));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<(string Name, string ItemId)>?> ListDirectories(string directory)
    {
        var itemId = await GetDriveItemId(directory);

        var result = await _client.Drives[_settings.DriveId].Items[itemId].Children.GetAsync();
        if (result?.Value == null)
        {
            _logger.LogError("Failed to retrieve list of files for {item}.", itemId);
            return null; //TODO: Throw exceptions and catch them nicely in controller.
        }

        var folders = result.Value.Where(r => r.Folder != null).ToList();

        return folders.Select(f => (f.Name, f.Id));
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
            return itemPath ?? IOneDriveClient.RootItemKey;

        //Get a list of all steps in the item path.
        var itemPathParts = new Uri(itemPath).Segments;
        var itemId = IOneDriveClient.RootItemKey;

        //Loop throught the path to find the final item ID.
        foreach (var pathPart in itemPathParts)
        {
            var result = await _client.Drives[_settings.DriveId].Items[itemId].Children.GetAsync();
            if (result?.Value == null)
                throw new Exception("Failed to get item ID.");

            var item = result.Value.FirstOrDefault(r => r.Name.Equals(pathPart));
            if (item == null)
                throw new Exception("Failed to get item ID.");

            itemId = item.Id;
        }

        //Return the final itemId.
        return itemId;
    }
}