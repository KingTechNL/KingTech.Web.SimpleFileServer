using KingTech.Web.SimpleFileServer.Abstract.Sources;

namespace KingTech.Web.SimpleFileServer.GitHubPlugins;

public class GitHubFileSourceSettings : IFileSourceSettings
{
    /// <inheritdoc/>
    public bool Enabled { get; set; }
    /// <inheritdoc/>
    public bool IsReadOnly { get; set; }
    /// <inheritdoc/>
    public string Name { get; set; }

    /// <summary>
    /// The type of connection this plugin will make to GitHub (using the GitHub API or plain Git clone/fetch/pull).
    /// </summary>
    public GitHubMode Mode { get; set; } = GitHubMode.Api;

    /// <summary>
    /// The owner of the GitHub repository to use as file source.
    /// </summary>
    public string Owner { get; set; }
    /// <summary>
    /// The GitHub repository to use as file source.
    /// </summary>
    public string Repository { get; set; }
    /// <summary>
    /// The repository branch to use.
    /// If left empty, the repository's default branch will be used.
    /// </summary>
    public string? Branch { get; set; }

    /// <summary>
    /// If using plain git: The local directory to clone the git repository in.
    /// </summary>
    public string? LocalDirectory { get; set; }

    /// <summary>
    /// If using plain git: The minimum time SimpleFileServer wait until checking if the local repository is up to date.
    /// Checking will only be done once information is requested.
    /// Default value is 1 minute.
    /// </summary>
    public TimeSpan CheckInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <inheritdoc/>
    public bool Verify(ref List<string> errors)
    {
        if(!Enabled)
            return true;

        var errorCount = errors.Count;

        if(string.IsNullOrWhiteSpace(Owner))
            errors.Add("No GitHub owner given.");
        if (string.IsNullOrWhiteSpace(Repository))
            errors.Add("No GitHub repository given.");
        if (Mode == GitHubMode.Git && string.IsNullOrWhiteSpace(LocalDirectory))
            errors.Add("LocalDirectory setting is required when using plain Git as source.");
        if (Mode == GitHubMode.Git && CheckInterval <= TimeSpan.Zero)
            errors.Add("CheckInterval must be a positive value.");

        return errorCount == errors.Count;
    }
}

/// <summary>
/// Enum describing the different modes this file source can use.
/// </summary>
public enum GitHubMode
{
    /// <summary>
    /// The official GitHub API.
    /// </summary>
    Api,
    /// <summary>
    /// The plain GIT protocol (clone/fetch/pull).
    /// </summary>
    Git
}