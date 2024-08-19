using KingTech.Web.SimpleFileServer.Abstract.Sources;

namespace KingTech.Web.SimpleFileServer.OneDrivePlugin;

public class OneDriveFileSourceSettings : IFileSourceSettings
{
    /// <inheritdoc/>
    public bool Enabled { get; set; } = false;
    /// <inheritdoc/>
    public bool IsReadOnly { get; set; } = true;

    /// <inheritdoc/>
    public string Name { get; set; }

    /// <summary>
    /// If set to true, the OneDriveFileSource will assume you are passing Microsoft Graph API Item ID's as file or directory names.
    /// If set to false, the OneDriveFileSource will try to convert the given file/directory names itself but this will come with some overhead.
    /// </summary>
    public bool UseItemId { get; set; } = true;

    /// <summary>
    /// The OneDrive Drive ID to get the files from.
    /// </summary>
    /// <remarks>Default drive ID 'me' refers to a personal onedrive.</remarks>
    public string? DriveId { get; set; } = "me";

    /// <summary>
    /// Credentials used for ClientSecret authentication with Microsoft (Graph) API's.
    /// </summary>
    public ClientSecretCredentialSettings? ClientSecretCredentials { get; set; }

    /// <summary>
    /// Credentials used for authorization code authentication with Microsoft (Graph/OneDrive) API's.
    /// </summary>
    public AuthorizationCodeSettings? AuthorizationCodeCredentials { get; set; }



    public bool Verify(ref List<string> errors)
    {
        var errorCount = errors.Count;

        //Check drive ID.
        if (string.IsNullOrWhiteSpace(DriveId))
            errors.Add("No DriveId given.");

        //Check client secret credentials (if used)
        if (ClientSecretCredentials != null)
        {
            if (string.IsNullOrWhiteSpace(ClientSecretCredentials.ClientId))
                errors.Add("No ClientId given.");
            if (string.IsNullOrWhiteSpace(ClientSecretCredentials.TenantId))
                errors.Add("No TenantId given.");
            if (string.IsNullOrWhiteSpace(ClientSecretCredentials.ClientSecret))
                errors.Add("No ClientSecret given.");
        }
        else if (AuthorizationCodeCredentials != null)
        {
            if(string.IsNullOrWhiteSpace(AuthorizationCodeCredentials.AuthorizationCode))
                errors.Add("No AuthorizationCode given.");
            if (string.IsNullOrWhiteSpace(AuthorizationCodeCredentials.ClientId))
                errors.Add("No ClientId given.");
            if (string.IsNullOrWhiteSpace(AuthorizationCodeCredentials.TenantId))
                errors.Add("No TenantId given.");
            if (string.IsNullOrWhiteSpace(AuthorizationCodeCredentials.ClientSecret))
                errors.Add("No ClientSecret given.");
        }
        else
        {
            errors.Add("No OneDrive credentials given.");
        }

        return errorCount == errors.Count;
    }
}

/// <summary>
/// Credentials used for ClientSecret authentication with Microsoft (Graph) API's.
/// </summary>
public class ClientSecretCredentialSettings
{
    /// <summary>
    /// The ID of the Client (app registration) that will be used for authentication.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// The Tenant the drive is part of.
    /// </summary>
    /// <remarks>Default tenant ID 'common' refers to a personal onedrive.</remarks>
    public string TenantId { get; set; } = "common";
    /// <summary>
    /// The Secret (auth token) that will be used for authentication.
    /// </summary>
    public string ClientSecret { get; set; }
}

/// <summary>
/// Credentials used for AuthorizationCode authentication with Microsoft (Graph/OneDrive) API's.
/// </summary>
public class AuthorizationCodeSettings
{
    /// <summary>
    /// The authorization code used for logging in to the OneDrive API.
    /// </summary>
    public string AuthorizationCode { get; set; }

    /// <summary>
    /// The ID of the Client (app registration) that will be used for authentication.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// The Tenant the drive is part of.
    /// </summary>
    /// <remarks>Default tenant ID 'common' refers to a personal onedrive.</remarks>
    public string TenantId { get; set; } = "common";
    /// <summary>
    /// The Secret (auth token) that will be used for authentication.
    /// </summary>
    public string ClientSecret { get; set; }
}