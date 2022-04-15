namespace KingTech.Web.SimpleFileServer.Plugins;

/// <summary>
/// Settings for plugin loading.
/// </summary>
public class PluginSettings
{
    /// <summary>
    /// Directories to load plugins from.
    /// </summary>
    public List<string> PluginDirectories { get; set; }

    /// <summary>
    /// Regexes to define which files/directories should be excluded from plugin loading.
    /// </summary>
    public List<string> ExcludeRegexes { get; set; } = new List<string>();
}
