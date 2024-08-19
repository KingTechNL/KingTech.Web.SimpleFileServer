using KingTech.Web.SimpleFileServer.Abstract.Sources;

namespace KingTech.Web.SimpleFileServer.GitHubPlugins;

public class GitHubFileSourceSettings : IFileSourceSettings
{
    /// <inheritdoc/>
    public bool Enabled { get; set; }
    /// <inheritdoc/>
    public bool IsReadOnly { get; set; }

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
    /// If using plain git: The local directory to clone the git repository in.
    /// </summary>
    public string? LocalDirectory { get; set; }

    /// <inheritdoc/>
    public bool Verify(ref List<string> errors)
    {
        return true;
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