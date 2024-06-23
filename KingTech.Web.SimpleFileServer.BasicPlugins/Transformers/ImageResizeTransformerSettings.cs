using KingTech.Web.SimpleFileServer.Abstract.Transformers;

namespace KingTech.Web.SimpleFileServer.BasicPlugins.Transformers;

/// <summary>
/// Settings for the <see cref="ImageResizeTransformer"/>.
/// </summary>
public class ImageResizeTransformerSettings : ITransformerSettings
{

    /// <summary>
    /// The width of the resulting image.
    /// </summary>
    public int ThumbnailWidth { get; set; } = 100;

    /// <summary>
    /// The height of the resulting image.
    /// </summary>
    public int ThumbnailHeight { get; set; } = 100;

    /// <summary>
    /// Whether or not to keep the image aspect ration intact.
    /// </summary>
    public bool KeepThumbnailAspectRatio { get; set; } = true;

    public bool Verify(ref List<string> errors)
    {
        var errorCount = errors.Count;

        if (ThumbnailWidth <= 0 || ThumbnailHeight <= 0)
            errors.Add("Target sizes must be greater then 0.");

        return errorCount == errors.Count;
    }
}