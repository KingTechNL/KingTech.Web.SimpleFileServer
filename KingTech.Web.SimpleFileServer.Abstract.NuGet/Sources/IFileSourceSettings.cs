namespace KingTech.Web.SimpleFileServer.Abstract.Sources;

/// <summary>
/// Settings for specific file source.
/// These settings are used to determine which file sources to load and pass specific settings to the file source.
/// </summary>
public interface IFileSourceSettings
{
    /// <summary>
    /// The name of the file source to load.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Method used to verify settings.
    /// </summary>
    /// <param name="errors">List of errors encountered while loading settings. Append to this list if any errors are encountered.</param>
    /// <returns>True if settings are valid, false otherwise.</returns>
    public bool Verify(ref List<string> errors);
}