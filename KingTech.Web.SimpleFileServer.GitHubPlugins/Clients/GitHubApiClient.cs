using Microsoft.Extensions.Logging;
using Octokit;

namespace KingTech.Web.SimpleFileServer.GitHubPlugins.Clients;

public class GitHubApiClient : IGitHubClient
{
    private const string RawFileBaseUrl = "https://raw.githubusercontent.com/";

    private readonly ILogger<GitHubApiClient> _logger;
    private readonly string _owner;
    private readonly string _repository;

    private readonly GitHubClient _gitHubClient;

    public GitHubApiClient(ILogger<GitHubApiClient> logger, string owner, string repository)
    {
        _logger = logger;
        _owner = owner;
        _repository = repository;

        _gitHubClient = new GitHubClient(new ProductHeaderValue("SimpleFileServer"));
    }

    public async Task<GitItem> GetItem(string filePath)
    {
        //var item = await _gitHubClient.Repository.Content.GetRawContent(_owner, _repository, filePath);
        var repo = await _gitHubClient.Repository.Get(_owner, _repository);
        var branch = repo.DefaultBranch; //TODO: Make branch configurable.
        var url = Path.Combine(RawFileBaseUrl, _owner, _repository, branch, filePath);

        

        return new GitItem()
        {
            Name = Path.GetFileName(filePath),
            Type = ItemType.File,
            Url = url, //TODO: I wont have this for the local github dir files...
            fileStream = await GetStreamFromUrl(url), //TODO: I can add streams for all files in the GetItems...
        };
    }

    public async Task<IEnumerable<GitItem>> GetItems(string directory)
    {
        var items = await _gitHubClient.Repository.Content.GetAllContents(_owner, _repository);
        if (items == null)
            throw new Exception("Failed to get content from GitHub API client.");

        var gitItems = items.Select(item => new GitItem() {Name = item.Name, Type = item.Type == ContentType.Dir ? ItemType.Directory : ItemType.File, Url = item.Url}).ToList();

        GetRateLimits();

        return gitItems;
    }

    private async Task<Stream> GetStreamFromUrl(string url)
    {
        using var client = new HttpClient();
        var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        var stream = await response.Content.ReadAsStreamAsync();
        return stream;
    }

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