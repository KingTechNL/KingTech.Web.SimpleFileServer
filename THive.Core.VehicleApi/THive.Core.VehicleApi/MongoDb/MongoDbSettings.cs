namespace THive.Core.DeviceApi.MongoDb;

/// <summary>
/// Settings for connecting to MongoDB. Either set the ConnectionString, or supply dats for the different properties.
/// </summary>
public class MongoDbSettings
{
    public string Hostname { get; set; } = "localhost";
    public int Port { get; set; } = 27017;
    public string Username { get; set; }
    public string Password { get; set; }
    public string DatabaseName { get; set; }

    public string ConnectionString { get; set; } = string.Empty;

    public bool HasUserNameAndPassword() => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);

    public string GetConnectionString() => string.IsNullOrEmpty(ConnectionString) ?
        GeneratedConnectionString : ConnectionString;

    private string GeneratedConnectionString => HasUserNameAndPassword() ? $"mongodb://{Username}:{Password}@{Hostname}:{Port}" : $"mongodb://{Hostname}:{Port}";
}