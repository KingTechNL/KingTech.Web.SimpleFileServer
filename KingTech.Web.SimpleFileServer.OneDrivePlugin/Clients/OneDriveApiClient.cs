using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Microsoft.Identity.Client;
using System.Diagnostics;

namespace KingTech.Web.SimpleFileServer.OneDrivePlugin.Clients;

/// <summary>
/// This <see cref="IOneDriveClient"/> uses the standard OneDrive API with an authorization token.
/// </summary>
/// <remarks>THIS CLASS IS STILL IN DEVELOPMENT.</remarks>
public class OneDriveApiClient : IOneDriveClient
{
    private readonly ILogger<OneDriveApiClient> _logger;
    private readonly OneDriveFileSourceSettings _settings;


    public OneDriveApiClient(ILogger<OneDriveApiClient> logger, OneDriveFileSourceSettings settings)
    {
        _logger = logger;
        _settings = settings;

    }

    /// <inheritdoc />
    public Task<Stream> GetFileStream(string fileName)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<(string Name, string ItemId)>> ListFiles(string directory)
    {
        //var accessToken = await RequestAccessToken();
        var accessToken = await RequestAuthorizationCode("www.google.com");

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Example: Get the root folder content
        //var response = await client.GetAsync("https://graph.microsoft.com/v1.0/me/drive/root/children");
        var response = await client.GetAsync("https://api.onedrive.com/v1.0/me/drive/root/children");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine(content);
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
        }

        return null;
    }

    /// <inheritdoc />
    public Task<IEnumerable<(string Name, string ItemId)>?> ListDirectories(string directory)
    {
        throw new NotImplementedException();
    }

    private async Task<string> RequestAccessToken()
    {
        using var client = new HttpClient();

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", _settings.AuthorizationCodeCredentials?.ClientId),
            new KeyValuePair<string, string>("client_secret", _settings.AuthorizationCodeCredentials?.ClientSecret),
            new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"),
        });

        try
        {
            var response =
                await client.PostAsync($"https://login.microsoftonline.com/{_settings.AuthorizationCodeCredentials?.TenantId}/oauth2/v2.0/token", content);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
                if (json != null)
                    return json.AccessToken;
                //string json = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(json); // Access token is in the "access_token" field
            }
            else
            {
                _logger.LogError("Failed to retrieve access token: {response}", response.StatusCode);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to retrieve access token.");
        }

        return string.Empty;
    }

    private async Task<string> RequestAccessToken(string authorizationCode, string redirectUri)
    {
        using var client = new HttpClient();

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("client_id", _settings.AuthorizationCodeCredentials?.ClientId),
            new KeyValuePair<string, string>("client_secret", _settings.AuthorizationCodeCredentials?.ClientSecret),
            new KeyValuePair<string, string>("scope", "Files.Read Files.ReadWrite"),
            new KeyValuePair<string, string>("code", authorizationCode),
            new KeyValuePair<string, string>("redirect_uri", redirectUri),
        });

        try
        {
            var response =
                await client.PostAsync($"https://login.microsoftonline.com/common/oauth2/v2.0/token",
                    content);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadFromJsonAsync<AuthenticationResponse>();
                if (json != null)
                    return json.AccessToken;
                //string json = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(json); // Access token is in the "access_token" field
            }
            else
            {
                _logger.LogError("Failed to retrieve access token: {response}", response.StatusCode);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to retrieve access token.");
        }

        return string.Empty;
    }


    private async Task<string> RequestAuthorizationCode(string redirectUri)
    {
        var pca = PublicClientApplicationBuilder
            .Create(_settings.AuthorizationCodeCredentials?.ClientId)
            .WithRedirectUri(redirectUri)
            .Build();

        var scopes = new[] { "Files.Read", "Files.ReadWrite" }; // Specify the required scopes

        try
        {
            var authResult = await pca.AcquireTokenInteractive(scopes).ExecuteAsync();
            Console.WriteLine($"Authorization code: {authResult.AccessToken}");
            return authResult.AccessToken;
        }
        catch (MsalException ex)
        {
            Debug.WriteLine($"Error acquiring token: {ex.Message}");
        }
        return string.Empty;
    }

    private class AuthenticationResponse
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("ext_expires_in")]
        public int ExtExpiresIn { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}