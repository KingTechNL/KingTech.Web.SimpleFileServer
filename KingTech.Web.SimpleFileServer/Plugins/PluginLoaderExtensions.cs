namespace KingTech.Web.SimpleFileServer.Plugins;

/// <summary>
/// Extension methods concerning the plugin loading.
/// </summary>
public static class PluginLoaderExtensions
{
    /// <summary>
    /// Get a specific configuration section from the configuration based on its name.
    /// </summary>
    /// <param name="configuration">The configuration to get the section from.</param>
    /// <typeparam name="TConfigurationSection">The type of the configuration section to retrieve.</typeparam>
    /// <returns>The requested configuration section from the configuration.</returns>
    public static TConfigurationSection GetConfiguration<TConfigurationSection>(this IConfiguration configuration)
    {
        var config = configuration.GetSection(typeof(TConfigurationSection).Name);
        if (config == null)
            return default;
        return config.Get<TConfigurationSection>();
    }
}
