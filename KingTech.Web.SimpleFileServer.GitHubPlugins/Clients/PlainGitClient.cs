using LibGit2Sharp;
using Microsoft.Extensions.Logging;

namespace KingTech.Web.SimpleFileServer.GitHubPlugins.Clients;

public class PlainGitClient : IGitHubClient
{
    private const string Remote = "remote";
    private const string SignatureName = "SimpleFileServer";
    private const string SignatureEmail = "your-email@example.com";

    private readonly ILogger<PlainGitClient> _logger;
    private readonly string _owner;
    private readonly string _repository;
    private readonly string? _branch;
    private readonly string _localDirectory;
    private readonly TimeSpan _checkInterval;

    private DateTime _lastRepositoryCheck = DateTime.MinValue;

    public PlainGitClient(ILogger<PlainGitClient> logger, string owner, string repository, string? branch, string localDirectory, TimeSpan checkInterval)
    {
        _logger = logger;
        _owner = owner;
        _repository = repository;
        _branch = branch;
        _localDirectory = localDirectory;
        _checkInterval = checkInterval;
    }

    public Task<Stream> GetFile(string itemPath)
    {
        //Check if we are up-to-date.
        if (!CheckIfLocalRepositoryIsUpToDate())
            Pull();

        var stream = File.Open(itemPath, FileMode.Open);
        return Task.FromResult((Stream) stream);
    }

    public Task<IEnumerable<string>> GetFiles(string directory)
    {
        //Check if we are up-to-date.
        if (!CheckIfLocalRepositoryIsUpToDate())
            Pull();

        var files = Directory.GetFiles(_localDirectory).Select(f => Path.GetFileName(f));

        return Task.FromResult(files);
    }

    public Task<IEnumerable<string>> GetDirectories(string directory)
    {
        //Check if we are up-to-date.
        if (!CheckIfLocalRepositoryIsUpToDate())
            Pull();

        var directories = Directory.GetDirectories(_localDirectory).Select(f => Path.GetFileName(f));

        return Task.FromResult(directories);
    }

    /// <summary>
    /// Pull the latest changes into the local directory.
    /// If the local directory does not contain a git project yet, clone the configured repository.
    /// </summary>
    private void Pull()
    {
        // Clone the repository if it doesn't exist locally
        if (!Repository.IsValid(_localDirectory))
        {
            _logger.LogInformation("Cloning repository {owner}/{repository} to {localDirectory}", _owner, _repository, _localDirectory);
            var cloneOptions = _branch == null ? null : new CloneOptions(){BranchName = _branch};
            Repository.Clone($"https://github.com/{_owner}/{_repository}.git", _localDirectory, cloneOptions);
        }

        // Fetch and pull the latest changes
        using (var repo = new Repository(_localDirectory))
        {
            var remote = repo.Network.Remotes[Remote];
            var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);

            Commands.Fetch(repo, remote.Name, refSpecs, null, null);
            Commands.Pull(repo, new Signature(SignatureName, SignatureEmail, DateTimeOffset.Now), null);
        }

        _logger.LogInformation("Repository fetched and pulled successfully.");
    }

    /// <summary>
    /// Check if our local repository is up to date with the remote repository.
    /// If this check is been performed within the set interval, this method will return true.
    /// If no local repository is detected in the local directory, this method will return false.
    /// </summary>
    /// <returns>True if the last commit in the local repository equals that of the remote repository, false otherwise.</returns>
    private bool CheckIfLocalRepositoryIsUpToDate()
    {
        //Skip if we have checked for an update too recently.
        if (DateTime.Now.Subtract(_lastRepositoryCheck) < _checkInterval)
        {
            return true;
        }
        _lastRepositoryCheck = DateTime.Now;

        //Check if directory contains git project.
        if (!Repository.IsValid(_localDirectory))
            return false;

        // Fetch the latest changes from the remote
        using var repo = new Repository(_localDirectory);
        var remote = repo.Network.Remotes[Remote];
        var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
        Commands.Fetch(repo, remote.Name, refSpecs, null, null);

        // Get the latest commit on the local branch
        var localCommit = repo.Head.Tip;

        // Get the latest commit on the remote branch
        var remoteBranch = repo.Branches[$"refs/remotes/{Remote}/{repo.Head.FriendlyName}"];
        var remoteCommit = remoteBranch.Tip;

        // Compare the commits
        if (localCommit.Sha == remoteCommit.Sha)
        {
            _logger.LogDebug("The local repository is up to date with the remote repository.");
            return true;
        }
        _logger.LogDebug("The local repository is not up to date with the remote repository.");
        return false;
    }
}