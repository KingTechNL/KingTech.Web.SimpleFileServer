using KingTech.Web.SimpleFileServer.Abstract.Models;
using KingTech.Web.SimpleFileServer.Abstract.Sources;
using KingTech.Web.SimpleFileServer.GitHubPlugins.Clients;
using Microsoft.Extensions.Logging;

namespace KingTech.Web.SimpleFileServer.GitHubPlugins;

public class GitHubFileSource : FileSourceBase<GitHubFileSourceSettings>
{
    private readonly ILogger<GitHubFileSource> _logger;
    private readonly IGitHubClient _client;

    public GitHubFileSource(ILoggerFactory loggerFactory, GitHubFileSourceSettings settings) : base(settings)
    {
        _logger = loggerFactory.CreateLogger<GitHubFileSource>();
        if (settings.Mode == GitHubMode.Api)
        {
            _client = new GitHubApiClient(loggerFactory.CreateLogger<GitHubApiClient>(), settings.Owner,
                settings.Repository);
        }
        else
        {
            _client = new PlainGitClient(loggerFactory.CreateLogger<PlainGitClient>(), settings.Owner,
                settings.Repository, settings.LocalDirectory, settings.CheckInterval);
        }
    }

    public override async Task<StoredFile> GetFile(string fileName)
    {
        var stream = await _client.GetFile(fileName);
        if (stream == null)
            return null;
        return new StoredFile(fileName, stream);
    }

    public override async Task<IEnumerable<string>> ListFiles(string? directory) => await _client.GetFiles(directory);

    public override async Task<IEnumerable<string>> ListDirectories(string? directory) => await _client.GetDirectories(directory);
}