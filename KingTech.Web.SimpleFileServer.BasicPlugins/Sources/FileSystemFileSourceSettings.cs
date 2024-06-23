using KingTech.Web.SimpleFileServer.Abstract.Sources;

namespace KingTech.Web.SimpleFileServer.BasicPlugins.Sources;

/// <summary>
/// Settings for the <see cref="FileSystemFileSource"/>
/// </summary>
public class FileSystemFileSourceSettings : IFileSourceSettings
{
    /// <inheritdoc/>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// The base directory files are stored.
    /// </summary>
    /// <remarks>When deploying as docker, this points to a directory in the docker container.</remarks>
    public string BaseDirectory { get; set; } = "/files";

    /// <inheritdoc/>
    public bool Verify(ref List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(BaseDirectory))
            errors.Add("No base directory given!");
        return !string.IsNullOrWhiteSpace(BaseDirectory);
    }
}