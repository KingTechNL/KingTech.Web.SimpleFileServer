using Microsoft.Extensions.Logging;
using Octokit;

namespace KingTech.Web.SimpleFileServer.GitHubPlugins.Clients;

public class GitHubApiClient : IGitHubClient
{
    //https://raw.githubusercontent.com/KingTechNL/KingTech.Web.SimpleFileServer/main/KingTech.Web.SimpleFileServer.BasicPlugins/Sources/FileSystemFileSource.cs
    private const string RawFileBaseUrl = "https://raw.githubusercontent.com/";

    private readonly ILogger<GitHubApiClient> _logger;
    private readonly string _owner;
    private readonly string _repository;
    private readonly string _branch;

    private readonly GitHubClient _gitHubClient;
    private readonly Repository _repo;

    public GitHubApiClient(ILogger<GitHubApiClient> logger, string owner, string repository, string? branch)
    {
        _logger = logger;
        _owner = owner;
        _repository = repository;

        _gitHubClient = new GitHubClient(new ProductHeaderValue("SimpleFileServer"));
        _repo = _gitHubClient.Repository.Get(_owner, _repository).Result; //TODO: May throw exception.
        _branch = branch ?? _repo.DefaultBranch;
    }

    /// <inheritdoc />
    public async Task<Stream> GetFile(string filePath)
    {
        //var item = await _gitHubClient.Repository.Content.GetRawContent(_owner, _repository, filePath);
        var url = Path.Combine(RawFileBaseUrl, _owner, _repository, _branch, filePath);

        return await GetStreamFromUrl(url);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetFiles(string directory)
    {
        var items = await _gitHubClient.Repository.Content.GetAllContentsByRef(_owner, _repository, directory, _branch);
        if (items == null)
            throw new Exception("Failed to get content from GitHub API client.");

        var gitItems = items.Where(item => item.Type == ContentType.File).Select(item => item.Name);

        GetRateLimits();

        return gitItems;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetDirectories(string directory)
    {
        var items = await _gitHubClient.Repository.Content.GetAllContentsByRef(_owner, _repository, directory, _branch);
        if (items == null)
            throw new Exception("Failed to get content from GitHub API client.");

        var gitItems = items.Where(item => item.Type == ContentType.Dir).Select(item => item.Name);

        GetRateLimits();

        return gitItems;
    }

    /// <summary>
    /// Create a stream from a URL.
    /// </summary>
    /// <param name="url">The URL to create a stream from.</param>
    /// <returns>The stream for the file the URL is pointing to.</returns>
    private async Task<Stream> GetStreamFromUrl(string url)
    {
        using var client = new HttpClient();
        var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        var stream = await response.Content.ReadAsStreamAsync();
        return stream;
    }

    /// <summary>
    /// Log the API limits for debug purposes.
    /// </summary>
    private void GetRateLimits()
    {
        // Prior to first API call, this will be null, because it only deals with the last call.
        var apiInfo = _gitHubClient.GetLastApiInfo();

        // If the ApiInfo isn't null, there will be a property called RateLimit
        var rateLimit = apiInfo?.RateLimit;

        var howManyRequestsCanIMakePerHour = rateLimit?.Limit;
        var howManyRequestsDoIHaveLeft = rateLimit?.Remaining;
        var whenDoesTheLimitReset = rateLimit?.Reset; // UTC time

        _logger.LogDebug("Hourly limit: {requestLimit}", howManyRequestsCanIMakePerHour);
        _logger.LogDebug("Requests left: {requestsRemaining}", howManyRequestsDoIHaveLeft);
        _logger.LogDebug("Next reset: {nextReset}", whenDoesTheLimitReset);
    }
}