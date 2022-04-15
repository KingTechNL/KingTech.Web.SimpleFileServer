namespace KingTech.Web.SimpleFileServer.Abstract.Transformers;

/// <summary>
/// Settings for specific transformer.
/// These settings are used to determine which transformers to load and pass specific settings to the transformer.
/// </summary>
public interface ITransformerSettings
{
    /// <summary>
    /// The name of the transformer to load.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Method used to verify settings.
    /// </summary>
    /// <param name="errors">List of errors encountered while loading settings. Append to this list if any errors are encountered.</param>
    /// <returns>True if settings are valid, false otherwise.</returns>
    public bool Verify(ref List<string> errors);
}