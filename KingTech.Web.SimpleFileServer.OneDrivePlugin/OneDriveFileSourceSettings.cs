using KingTech.Web.SimpleFileServer.Abstract.Sources;

namespace KingTech.Web.SimpleFileServer.OneDrivePlugin;

public class OneDriveFileSourceSettings : IFileSourceSettings
{
    public static string[] Scopes = { "https://graph.microsoft.com/.default" };

    /// <inheritdoc/>
    public bool IsReadOnly { get; set; } = true;

    /// <summary>
    /// The OneDrive Drive ID to get the files from.
    /// </summary>
    /// <remarks>Default drive ID 'me' refers to personal onedrive.</remarks>
    public string DriveId { get; set; } = "me";

    /// <summary>
    /// The ID of the Client (user) that will be used for authentication.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// The Tenant the Client is part of.
    /// </summary>
    /// <remarks>Default tenant ID 'personal' refers to personal onedrive.</remarks>
    public string TenantId { get; set; } = "Personal";
    /// <summary>
    /// The Secret (auth token) that will be used for authentication.
    /// </summary>
    public string ClientSecret { get; set; }

    /// <summary>
    /// If set to true, the OneDriveFileSource will assume you are passing Microsoft Graph API Item ID's as file or directory names.
    /// If set to false, the OneDriveFileSource will try to convert the given file/directory names itself but this will come with some overhead.
    /// </summary>
    public bool UseItemId { get; set; } = true;

    public bool Verify(ref List<string> errors)
    {
        var errorCount = errors.Count;

        if (string.IsNullOrWhiteSpace("DriveId"))
            errors.Add("No DriveId given.");
        if (string.IsNullOrWhiteSpace("ClientId"))
            errors.Add("No ClientId given.");
        if (string.IsNullOrWhiteSpace("TenantId"))
            errors.Add("No TenantId given.");
        if (string.IsNullOrWhiteSpace("ClientSecret"))
            errors.Add("No ClientSecret given.");

        return errorCount == errors.Count;
    }
}