using THive.Core.DeviceApi.DbStorage.MongoDb;
using THive.Core.DeviceApi.DbStorage.MongoDb.Extensions;

namespace THive.Core.DeviceApi;

public static class WebServiceExtensions
{
    /// <summary>
    /// Load the given configuration/settings type using the ConfigurationManager and register it to the DI container.
    /// </summary>
    /// <typeparam name="TConfiguration">Type of configuration to load.</typeparam>
    /// <param name="builder">WebApplicationBuilder containing DI container and ConfigurationManager</param>
    /// <param name="optional">If set to true, this settings object is optional and doesnt throw an error when not existing.</param>
    public static TConfiguration Configure<TConfiguration>(this WebApplicationBuilder builder, bool optional = false) where TConfiguration : class
    {
        var config = builder.Configuration.GetSection(typeof(TConfiguration).Name);
        if (!optional && config?.Get<TConfiguration>() == null)
            throw new Exception($"No {typeof(TConfiguration).Name} found in settings file!");
        builder.Services.Configure<TConfiguration>(config);

        if (config?.Get<TConfiguration>() != null)
        {
            var configuration = config.Get<TConfiguration>();
            builder.Services.AddSingleton<TConfiguration>(configuration);
            return configuration;
        }

        return default;
    }

    /// <summary>
    /// Load the given configuration/settings type using the ConfigurationManager and register it to the DI container.
    /// </summary>
    /// <typeparam name="TConfiguration">Type of configuration to load.</typeparam>
    /// <param name="builder">WebApplicationBuilder containing DI container and ConfigurationManager</param>
    /// <param name="defaultValue">Default value to use if no configuration could be loaded.</param>
    public static TConfiguration Configure<TConfiguration>(this WebApplicationBuilder builder, TConfiguration defaultValue) where TConfiguration : class
    {
        var config = builder.Configuration.GetSection(typeof(TConfiguration).Name);
        if (config?.Get<TConfiguration>() != null)
        {
            var configuration = config.Get<TConfiguration>();
            builder.Services.Configure<TConfiguration>(config);
            builder.Services.AddSingleton<TConfiguration>(configuration);
            return configuration;
        }
        else
        {
            builder.Services.AddSingleton<TConfiguration>(defaultValue);
            return defaultValue;
        }
    }

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

    /// <summary>
    /// Register an implementation of given type to the DI container.
    /// </summary>
    /// <typeparam name="TInterface">The interface to register an implementation for.</typeparam>
    /// <typeparam name="TImplementation">The implementation type to register.</typeparam>
    /// <param name="builder">The builder to register the class to.</param>
    /// <param name="lifeStyle">The lifeStyle for this registration (default = Transient).</param>
    public static void Register<TInterface, TImplementation>(this WebApplicationBuilder builder, ServiceLifetime lifeStyle = ServiceLifetime.Transient)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        switch (lifeStyle)
        {
            case ServiceLifetime.Singleton:
                builder.Services.AddSingleton<TInterface, TImplementation>();
                break;
            case ServiceLifetime.Scoped:
                builder.Services.AddScoped<TInterface, TImplementation>();
                break;
            case ServiceLifetime.Transient:
                builder.Services.AddTransient<TInterface, TImplementation>();
                break;
        }
    }

    /// <summary>
    /// Add a CORS policy based on the set CorsSettings.
    /// </summary>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to add the policies to.</param>
    /// <param name="policyName">The (unique) name of the CORS policy. Default = "CorsPolicy"</param>
    /// <typeparam name="TCorsSettings">The type of the <see cref="CorsSettings"/> to base the policy on. Type is passed to support inheritance.</typeparam>
    /// <returns>True if a CORS policy was added, false if no CORS settings where found.</returns>
    public static bool AddCorsPolicy<TCorsSettings>(this WebApplicationBuilder builder, string policyName = "CorsPolicy")
        where TCorsSettings : CorsSettings
    {
        var corsSettings = builder.Configuration.GetConfiguration<TCorsSettings>();

        //Allow all origins.
        if (corsSettings?.AllowAllOrigins ?? false)
        {
            builder.Services.AddCors(options =>
                options.AddPolicy(policyName,
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                ));
            return true;
        }

        //Allow specific origins.
        if (corsSettings?.AllowedOrigins?.Any() ?? false)
        {
            builder.Services.AddCors(options =>
                options.AddPolicy(policyName,
                    builder => builder
                        .WithOrigins(corsSettings.AllowedOrigins.ToArray())
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                ));
            return true;
        }

        //No CORS policy configured.
        return false;
    }

    /// <summary>
    /// Registers the given context as a service in the <see cref="IServiceCollection" />.
    /// </summary>
    /// <typeparam name="TContextImplementation">The concrete implementation type to create.</typeparam>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to add the service to.</param>
    /// <param name="contextLifetime">The lifetime with which to register the DbContext service in the container.</param>
    public static void AddMongoDbContext<TContextImplementation>(this WebApplicationBuilder builder, ServiceLifetime contextLifetime = ServiceLifetime.Scoped)
        where TContextImplementation : MongoDbContext 
        => builder.AddMongoDbContext<TContextImplementation, TContextImplementation>();

    /// <summary>
    /// Registers the given context as a service in the <see cref="IServiceCollection" />.
    /// </summary>
    /// <typeparam name="TContextService"> The class or interface that will be used to resolve the context from the container. </typeparam>
    /// <typeparam name="TContextImplementation"> The concrete implementation type to create. </typeparam>
    /// <param name="builder">The <see cref="WebApplicationBuilder"/> to add the service to.</param>
    /// <param name="contextLifetime">The lifetime with which to register the DbContext service in the container.</param>
    public static void AddMongoDbContext<TContextService, TContextImplementation>(this WebApplicationBuilder builder, ServiceLifetime contextLifetime = ServiceLifetime.Scoped)
        where TContextService : class
        where TContextImplementation : MongoDbContext, TContextService
    {
        var config = builder.Configuration.GetSection(nameof(MongoDbSettings));
        builder.Services.AddMongoDbContext<TContextService, TContextImplementation>(config);
    }
}