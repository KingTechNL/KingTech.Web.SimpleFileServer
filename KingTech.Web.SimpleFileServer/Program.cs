using KingTech.Web.SimpleFileServer.Abstract.Sources;
using KingTech.Web.SimpleFileServer.Abstract.Transformers;
using KingTech.Web.SimpleFileServer.Plugins;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Load transformer plugins.
var config = builder.Configuration.GetConfiguration<PluginSettings>();
var pluginDirectories = config?.PluginDirectories;
if (!pluginDirectories?.Any() ?? true)
    //pluginDirectories = new List<string>() { Assembly.GetEntryAssembly().Location };
    pluginDirectories = new List<string>() { "/plugins" };
var excludeRegexes = config?.ExcludeRegexes ?? new List<string>();
new PluginBuilder(LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        }).CreateLogger<PluginBuilder>())
    .AddPluginDirectory(pluginDirectories.ToArray())
    .AddExclusionRegex(excludeRegexes.ToArray())
    .AddPluginType<ITransformer>()
    .AddPluginType<IFileSource>()
    .AddPluginSettingsType<ITransformerSettings>()
    .AddPluginSettingsType<IFileSourceSettings>()
    .LoadPlugins(builder.Services, builder.Configuration);

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("ENABLE_SWAGGER")?.ToLower() == "true")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
