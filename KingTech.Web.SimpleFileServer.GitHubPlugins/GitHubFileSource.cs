using KingTech.Web.SimpleFileServer.Abstract.Models;
using KingTech.Web.SimpleFileServer.Abstract.Sources;
using Microsoft.Extensions.Logging;

namespace KingTech.Web.SimpleFileServer.GitHubPlugins;

public class GitHubFileSource : FileSourceBase<GitHubFileSourceSettings>
{
    public GitHubFileSource(ILogger<GitHubFileSource> logger, GitHubFileSourceSettings settings) : base(settings)
    {
        
    }

    public override Task<StoredFile> GetFile(string fileName)
    {
        throw new NotImplementedException();
    }

    public override Task<IEnumerable<string>> ListFiles(string? directory)
    {
        throw new NotImplementedException();
    }

    public override Task<IEnumerable<string>> ListDirectories(string? directory)
    {
        throw new NotImplementedException();
    }
}