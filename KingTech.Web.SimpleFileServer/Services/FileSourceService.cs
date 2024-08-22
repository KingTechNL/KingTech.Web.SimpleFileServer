using KingTech.Web.SimpleFileServer.Abstract.Models;
using KingTech.Web.SimpleFileServer.Abstract.Sources;

namespace KingTech.Web.SimpleFileServer.Services;

/// <summary>
/// This service is responsible for interacting with the registered FileSource plugins.
/// </summary>
public class FileSourceService : IFileSourceService
{
    private readonly ILogger<FileSourceService> _logger;
    private readonly GeneralSettings _generalSettings;
    private readonly IEnumerable<IFileSource> _sources;

    /// <summary>
    /// This service is responsible for interacting with the registered FileSource plugins.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> responsible for logging from this class.</param>
    /// <param name="generalSettings">Settings used to redirect requests to the correct file source.</param>
    /// <param name="sources">A list of <see cref="IFileSource"/> implementations.</param>
    public FileSourceService(ILogger<FileSourceService> logger, GeneralSettings generalSettings, IEnumerable<IFileSource> sources)
    {
        _logger = logger;
        _generalSettings = generalSettings;
        _sources = sources.Where(s => s.Enabled);
    }

    /// <summary>
    /// Get a list of file names in the given directory.
    /// </summary>
    /// <param name="directory">The directory to look in, if no directory is given we use the root dir.</param>
    /// <returns>A list of file names in the given directory.</returns>
    public async Task<IEnumerable<string>> GetFiles(string? directory)
    {
        //Get the sources to use for this request.
        var sources = GetSources(directory, out var pureDirectory);

        //Combine files from all sources.
        var files = new List<string>();
        foreach (var source in sources)
        {
            var sourceFiles = await source.ListFiles(pureDirectory);
            if (sourceFiles != null && sourceFiles.Any())
                files.AddRange(sourceFiles);
        }
        return files;
    }

    /// <summary>
    /// Get a list of directory names in the given directory.
    /// </summary>
    /// <param name="directory">The directory to look in, if no directory is given we use the root dir.</param>
    /// <returns>A list of directory names in the given directory.</returns>
    public async Task<IEnumerable<string>> GetDirectories(string? directory)
    {
        //Get the sources to use for this request.
        var sources = GetSources(directory, out var pureDirectory);

        //If no source name is given while this is required, return a list of source names.
        if (string.IsNullOrWhiteSpace(directory) && _generalSettings.PrefixSourceName && _sources.Count() > 1)
            return _sources.Select(s => s.Name);

        //Combine directories from all sources.
        var directories = new List<string>();
        foreach (var src in sources)
        {
            var sourceDirectories = await src.ListDirectories(pureDirectory);
            if (sourceDirectories != null && sourceDirectories.Any())
                directories.AddRange(sourceDirectories);
        }
        return directories;
    }

    /// <summary>
    /// Load the file using the registered <see cref="IFileSource"/>s.
    /// </summary>
    /// <param name="fileName">File path/name.</param>
    /// <returns>The loaded file including metadata, or null if no such file was found.</returns>
    public async Task<StoredFile> GetFile(string fileName)
    {
        //Get the sources to use for this request.
        var sources = GetSources(fileName, out var pureFileName);
        if (sources == null)
            return null;

        //Look through all resulting sources.
        StoredFile file = null;
        foreach (var source in sources)
        {
            file = await source.GetFile(pureFileName);
            if (file != null)
                break;
        }

        //File could not be found in any source / given source could not be found.
        return file;
    }

    /// <summary>
    /// Get the <see cref="IFileSource"/>s to use for the given FilePath.
    /// If this method returns null, there either are no elegible sources, the service is not configured correctly or the received path is incorrect.
    /// </summary>
    /// <param name="pathWithSource">The full file/directory path including source (if needed).</param>
    /// <param name="path">The path that can be passed to the returned file sources.</param>
    /// <returns>One or more <see cref="IFileSource"/>s that can be used for the given path, null if no such source could be found.</returns>
    private IEnumerable<IFileSource>? GetSources(string? pathWithSource, out string? path)
    {
        //If the path shouldn't contain the source name as prefix, return all sources.
        if (!_generalSettings.PrefixSourceName)
        {
            path = pathWithSource;
            return _sources;
        }

        //If path should contain source name and path is empty, but we have only one file source. Return that.
        if (string.IsNullOrWhiteSpace(pathWithSource) && _sources.Count() == 1)
        {
            path = pathWithSource;
            return _sources;
        }

        //If path should contain source name and path is empty, we have a problem.
        if (string.IsNullOrWhiteSpace(pathWithSource))
        {
            path = pathWithSource;
            return null;
        }

        //Get all path parts.
        var pathParts = pathWithSource.Split(Path.DirectorySeparatorChar, '/');
        var sourceName = pathParts[0];

        //If path should contain source name but only contains a file name, we might have a problem.
        if (pathParts.Length == 1 && !string.IsNullOrWhiteSpace(Path.GetExtension(sourceName)))
        {
            //If we only have one source. Just use that one.
            if (_sources.Count() == 1)
            {
                path = pathWithSource;
                return _sources;
            }

            //We don't know what source to use.
            _logger.LogWarning("FilePath {path} does not contain a source name.", pathWithSource);
            path = null;
            return null;
        }

        //Combine the other parts of the path back into a filepath.
        path = Path.Combine(pathParts.ToList().GetRange(1, pathParts.Length - 1).ToArray());

        //Try to get the filesource and return.
        var source = _sources.FirstOrDefault(s => s.Name.ToLower().Equals(sourceName.ToLower()));
        if (source == null)
            _logger.LogError("Unknown source requested: {source}", sourceName);
        return new List<IFileSource>() { source };
    }
}