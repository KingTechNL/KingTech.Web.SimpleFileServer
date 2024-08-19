using KingTech.Web.SimpleFileServer.Abstract.Models;

namespace KingTech.Web.SimpleFileServer.Abstract.Sources;

/// <summary>
/// Base class for SimpleFileServer FileSources.
/// </summary>
/// <typeparam name="TSettings">Type of the settings specific to this implementation.</typeparam>
public abstract class FileSourceBase<TSettings> : IFileSource
    where TSettings : class, IFileSourceSettings
{
    /// <inheritdoc />
    public virtual bool Enabled => Settings.Enabled;
    /// <inheritdoc />
    public virtual bool IsReadOnly => Settings.IsReadOnly;

    /// <inheritdoc />
    public virtual string Name => string.IsNullOrWhiteSpace(Settings.Name) ? this.GetType().Name.Replace("FileSource", "") : Settings.Name;

    /// <summary>
    /// The <see cref="IFileSourceSettings"/> for this specific implementation.
    /// </summary>
    protected TSettings Settings { get; }

    /// <summary>
    /// Base constructor taking <see cref="IFileSourceSettings"/> for specific implementation as parameter.
    /// </summary>
    /// <param name="settings">The <see cref="IFileSourceSettings"/> for this specific implementation.</param>
    protected FileSourceBase(TSettings settings)
    {
        Settings = settings;
    }

    /// <inheritdoc />
    public abstract Task<StoredFile> GetFile(string fileName);

    /// <inheritdoc />
    public abstract Task<IEnumerable<string>> ListFiles(string? directory);

    /// <inheritdoc />
    public abstract Task<IEnumerable<string>> ListDirectories(string? directory);
}