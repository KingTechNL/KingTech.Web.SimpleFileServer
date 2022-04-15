using System.Reflection;
using System.Text.RegularExpressions;

namespace KingTech.Web.SimpleFileServer.Plugins;

/// <summary>
/// The PluginBuilder is used by the main application to load in all plugins.
/// </summary>
public class PluginBuilder
{
    private readonly List<string> _pluginDirectories = new();
    private readonly List<string> _exclusionRegexes = new();
    private readonly List<Type> _pluginTypes = new();
    private readonly List<Type> _pluginSettingsTypes = new();
    private readonly ILogger<PluginBuilder> _logger;

    /// <summary>
    /// The PluginBuilder is used by the main application to load in all plugins.
    /// </summary>
    /// <param name="logger">Logger for logging plugin loader progress.</param>
    /// <param name="builder">The WebApplicationBuilder to register plugins and configurations to.</param>
    public PluginBuilder(ILogger<PluginBuilder> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Set the types of the plugins to load.
    /// </summary>
    /// <param name="pluginTypes"></param>
    /// <returns></returns>
    public PluginBuilder AddPluginTypes(params Type[] pluginTypes)
    {
        _pluginTypes.AddRange(pluginTypes);
        return this;
    }

    /// <summary>
    /// Add a plugin to register.
    /// </summary>
    /// <typeparam name="TPlugin">The type of plugin to register.</typeparam>
    /// <returns>this</returns>
    public PluginBuilder AddPluginType<TPlugin>()
    {
        _pluginTypes.Add(typeof(TPlugin));
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TPluginSettings"></typeparam>
    /// <returns></returns>
    public PluginBuilder AddPluginSettingsType<TPluginSettings>()
    {
        _pluginSettingsTypes.Add(typeof(TPluginSettings));
        return this;
    }

    /// <summary>
    /// This is the directory that is being scanned for plugins.
    /// </summary>
    /// <param name="pluginDirectory">The directory to scan for plugins.</param>
    /// <returns>this</returns>
    public PluginBuilder AddPluginDirectory(params string[] pluginDirectory)
    {
        _pluginDirectories.AddRange(pluginDirectory);
        return this;
    }

    /// <summary>
    /// Add regexes for files/directories to exclude from plugin loading.
    /// </summary>
    /// <param name="regex">The regex(es) to add.</param>
    /// <returns>this</returns>
    public PluginBuilder AddExclusionRegex(params string[] regex)
    {
        _exclusionRegexes.AddRange(regex);
        return this;
    }

    /// <summary>
    /// Load all plugins of the given types from the given assemblies.
    /// </summary>
    public void LoadPlugins(IServiceCollection services, IConfiguration configuration)
    {
        var assemblies = new List<Assembly>();
        //Load all assemblies.
        if (!_pluginDirectories?.Any() ?? false)
            _logger?.LogWarning("No assemblies will be added as no plugin directories have been given.");
        foreach (var pluginDirectory in _pluginDirectories)
        {
            var directoryAssemblies = LoadAssembliesInDirectory(services, pluginDirectory);
            assemblies.AddRange(directoryAssemblies);
            _logger?.LogInformation("{assemblyAmount} assemblies loaded from {pluginDirectory}.", directoryAssemblies?.Count() ?? 0, pluginDirectory);
        }

        //Register all plugins.
        if (!_pluginTypes?.Any() ?? false)
            _logger?.LogWarning("No plugins will be registered as no plugin types have been given.");
        foreach (var pluginType in _pluginTypes)
        {
            try
            {
                //Get all concrete, non generic implementations from assemblies.
                var assemblyTypes = assemblies.SelectMany(a => a.GetTypes());
                var pluginImplementations = assemblyTypes.Where(x =>
                   pluginType.IsAssignableFrom(x) //Type must be subtype of pluginType
                   && !x.IsAbstract  //Type cannot be abstract (since we cant initialize those)
                   && !x.IsGenericTypeDefinition  //Type cannot be generic
                ).ToList();

                //Register all implementations of pluginType to services.
                pluginImplementations.ForEach((t) =>
                {
                    services.AddTransient(pluginType, t);
                    _logger?.LogInformation("{type} plugin loaded", t);
                });
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Cannot register type: {type} from selected assemblies", pluginType);
            }
            _logger?.LogInformation("Plugin loading completed for {type}", pluginType);
        }

        //Load all settings
        foreach (var pluginSettingsType in _pluginSettingsTypes)
        {
            try
            {
                //Get all concrete, non generic implementations from assemblies.
                var assemblyTypes = assemblies.SelectMany(a => a.GetTypes());
                var pluginImplementations = assemblyTypes.Where(ast =>
                    !ast.IsInterface && !ast.IsGenericType && pluginSettingsType.IsAssignableFrom(ast)).ToList();

                //Load all implementations for this type from the settings and add them as singleton.
                pluginImplementations.ForEach(t => LoadSettings(t, services, configuration));
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Cannot register type: {type} from selected assemblies", pluginSettingsType);
            }
            _logger?.LogInformation("Plugin settings loading completed for {type}", pluginSettingsType);
        }
    }

    /// <summary>
    /// Load the settings from the configuration.
    /// If settings are not found, a default is generated.
    /// </summary>
    /// <param name="settingsType">The type of settings to load.</param>
    /// <param name="services">DI container to register settings.</param>
    /// <param name="configuration">Configuration to get settings from.</param>
    private void LoadSettings(Type settingsType, IServiceCollection services, IConfiguration configuration)
    {
        var loadedConfig = configuration.GetSection(settingsType.Name)?.Get(settingsType);
        if (loadedConfig != null)
        {
            services.AddSingleton(settingsType, loadedConfig);
            _logger?.LogInformation("Settings loaded for {type}: {@Config}", settingsType, loadedConfig);
        }
        else
        {
            try
            {
                var generatedConfig = Activator.CreateInstance(settingsType);
                services.AddSingleton(settingsType, generatedConfig);
                _logger?.LogInformation("No settings found for {type}, generated defaults: {@Config}", settingsType, generatedConfig);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "No settings found for {type} and failed to generate defaults.", settingsType);
            }
        }
    }

    /// <summary>
    /// Loads in all assemblies in the specified folder. Sharing all the types registered in the main assembly.
    /// This removes the need for hard project/nuget references.
    /// </summary>
    /// <param name="services">The container to load assemblies to.</param>
    /// <param name="assemblyDirectory">The directory to load assemblies from.</param>
    /// <returns>A list containing all loaded assemblies.</returns>
    private IEnumerable<Assembly> LoadAssembliesInDirectory(IServiceCollection services, string assemblyDirectory)
    {
        var retAssemblies = new List<Assembly>();
        if (!Directory.Exists(assemblyDirectory))
            assemblyDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), assemblyDirectory);
        if (!Directory.Exists(assemblyDirectory))
            return retAssemblies;//empty


        //Get all dlls
        foreach (var fileInfo in new DirectoryInfo(assemblyDirectory).GetFiles("*.dll", SearchOption.AllDirectories))
        {
            try
            {
                if (!fileInfo.Name.ToLower().Contains("sni.dll") //exclude the sni dlls (bad format exception)
                    && !fileInfo.Name.ToLower().StartsWith("system.") //exclude system and ms dll too. Cannot be used for plugin loading
                    && !fileInfo.Name.ToLower().StartsWith("microsoft.")
                    && !Exclude(fileInfo))
                {
                    var sharedTypes = services.Select(reg => reg.ServiceType).ToList();
                    sharedTypes.AddRange(_pluginTypes);
                    var loader = McMaster.NETCore.Plugins.PluginLoader.CreateFromAssemblyFile(fileInfo.FullName, sharedTypes.ToArray()); //get all registrations from the dicontainer and share
                    if (loader.LoadDefaultAssembly().GetTypes().Any()) //get the types (if none, don't use it. Will also check if all types dependencies check out)
                        retAssemblies.Add(loader.LoadDefaultAssembly());
                }
            }
            catch (Exception e)
            {
                _logger?.LogDebug(e, "Failed to load assembly {filename}", fileInfo.Name);
                //ignore
            }
        }
        return retAssemblies;
    }

    /// <summary>
    /// Check if the given file matches any of the set exclusion regexes.
    /// </summary>
    /// <param name="fileInfo">The file to check for.</param>
    /// <returns>True if the file matches any of the set exclusion regexes, false otherwise.</returns>
    private bool Exclude(FileInfo fileInfo)
    {
        try
        {
            return _exclusionRegexes.Select(regex => new Regex(regex)).Any(regex => regex.IsMatch(fileInfo.Name));
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Unable to apply exclusion regexes on {@FileInfo}", fileInfo);
            return false;
        }
    }
}
