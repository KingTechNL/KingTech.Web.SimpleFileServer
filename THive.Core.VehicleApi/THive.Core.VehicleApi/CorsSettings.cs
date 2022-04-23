namespace THive.Core.DeviceApi;

/// <summary>
/// Settings for CORS policy.
/// </summary>
public class CorsSettings
{
    /// <summary>
    /// If set to true, all origins are allowed to connect to this service.
    /// </summary>
    public bool AllowAllOrigins { get; set; }

    /// <summary>
    /// A list of origin 'urls' that are allowed to connect to this service.
    /// </summary>
    public IEnumerable<string> AllowedOrigins { get; set; } = new List<string>();
}