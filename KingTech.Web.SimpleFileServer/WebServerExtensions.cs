namespace KingTech.Web.SimpleFileServer;

public static class WebServerExtensions
{
    /// <summary>
    /// Load the given configuration/settings type using the ConfigurationManager and register it to the DI container.
    /// </summary>
    /// <typeparam name="TConfiguration">Type of configuration to load.</typeparam>
    /// <param name="builder">WebApplicationBuilder containing DI container and ConfigurationManager</param>
    public static void Configure<TConfiguration>(this WebApplicationBuilder builder) where TConfiguration : class, new()
    {
        var config = builder.Configuration.GetSection(typeof(TConfiguration).Name);
        builder.Services.Configure<TConfiguration>(config);
        builder.Services.AddSingleton<TConfiguration>(config.Get<TConfiguration>() ?? new TConfiguration());
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
}