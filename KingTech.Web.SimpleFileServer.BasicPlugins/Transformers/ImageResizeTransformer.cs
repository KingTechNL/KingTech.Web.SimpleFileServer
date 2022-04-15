using KingTech.Web.SimpleFileServer.Abstract.Models;
using KingTech.Web.SimpleFileServer.Abstract.Transformers;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Pbm;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace KingTech.Web.SimpleFileServer.BasicPlugins.Transformers;

/// <summary>
/// This transformer resizes images based on the passed settings.
/// <seealso cref="ITransformer"/>
/// </summary>
public class ImageResizeTransformer : ITransformer
{
    private readonly ILogger<ImageResizeTransformer> _logger;
    private readonly ImageResizeTransformerSettings _settings;

    /// <inheritdoc cref="ITransformer"/>
    public string Name { get; } = "ImageResize";

    /// <summary>
    /// Transformer to resize images.
    /// </summary>
    /// <param name="logger"><see cref="ILogger"/></param>
    /// <param name="settings"><see cref="ImageResizeTransformerSettings"/></param>
    public ImageResizeTransformer(ILogger<ImageResizeTransformer> logger, ImageResizeTransformerSettings settings)
    {
        _logger = logger;
        _settings = settings;
    }

    /// <summary>
    /// Check whether or not this transformer should be applied on the file.
    /// </summary>
    /// <param name="fileName">Name of the file, as requested by the endpoint.</param>
    /// <param name="storedFile">StoredFile as retrieved from the file source.</param>
    /// <returns>True if transformer should be applied to the file, false otherwise.</returns>
    public bool Match(string fileName, StoredFile storedFile)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        if (!nameWithoutExtension.EndsWith(_settings.PostFix))
            return false;
        if (storedFile == null)
            return false;
        if (GetEncoder(storedFile.Extension) == null)
            return false;
        return true;
    }

    /// <summary>
    /// Clean up a filename as received from the user-endpoint based on the postfix or other identifiers needed by this transformer.
    /// </summary>
    /// <param name="fileName">The filename to clean up.</param>
    /// <returns>The filename trimmed from all information needed for this transformer.</returns>
    public string GetCleanFileName(string fileName)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        if(!nameWithoutExtension.EndsWith(_settings.PostFix))
            return fileName;

        var cleanedName = nameWithoutExtension.Remove(nameWithoutExtension.Length - _settings.PostFix.Length);
        return $"{cleanedName}{extension}";
    }
        

    /// <summary>
    /// Resize the given image file based on the target size set in the settings.
    /// </summary>
    /// <param name="fileName">Name of the file, as requested by the endpoint.</param>
    /// <param name="storedFile">StoredFile as retrieved from the file source.</param>
    /// <returns>True on success, false otherwise.</returns>
    public bool Transform(string fileName, StoredFile storedFile)
    {
        _logger.LogInformation("Resizing {fileName}: {@File} to {w}x{h} ({keepAspect}keeping aspect ratio)", 
            fileName, storedFile, _settings.TargetWidth, _settings.TargetHeight, _settings.KeepAspectRatio ? "" : "not ");

        try
        {
            var image = SixLabors.ImageSharp.Image.Load(storedFile.File);
            image.Mutate(m => m.Resize(new ResizeOptions()
            {
                Size = new SixLabors.ImageSharp.Size(_settings.TargetWidth, _settings.TargetHeight)
            }));

            var resizedImage = new MemoryStream();
            //image.DetectEncoder(storedFile.Location);
            image.Save(resizedImage, GetEncoder(storedFile.Extension));
            
            storedFile.File = resizedImage;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resize image {fileName}", fileName);
            return false;
        }
    }

    /// <summary>
    /// Get the encoder used for the given file extension.
    /// </summary>
    /// <param name="extension">The extension to find an encoder for.</param>
    /// <returns>The encoder required for the given extension. Null if no such encoder was found.</returns>
    private IImageEncoder? GetEncoder(string extension) => 
        extension switch
        {
            ".png" => new PngEncoder(),
            ".jpg" => new JpegEncoder(),
            ".bmp" => new BmpEncoder(),
            ".gif" => new GifEncoder(),
            ".pbm" => new PbmEncoder(),
            ".tga" => new TgaEncoder(),
            ".tiff" => new TiffEncoder(),
            ".webp" => new WebpEncoder(),
            _ => null
        };
}


/// <summary>
/// Settings for the <see cref="ImageResizeTransformer"/>.
/// </summary>
public class ImageResizeTransformerSettings : ITransformerSettings
{
    /// <inheritdoc/>
    public string Name { get; set; }
    
    /// <summary>
    /// If the requested filename ends with this string, this transformer will be applied.
    /// </summary>
    public string PostFix { get; set; } = "_thumb";

    /// <summary>
    /// The width of the resulting image.
    /// </summary>
    public int TargetWidth { get; set; } = 100;

    /// <summary>
    /// The height of the resulting image.
    /// </summary>
    public int TargetHeight { get; set; } = 100;

    /// <summary>
    /// Whether or not to keep the image aspect ration intact.
    /// </summary>
    public bool KeepAspectRatio { get; set; } = true;

    public bool Verify(ref List<string> errors)
    {
        var errorCount = errors.Count;

        if(TargetWidth <= 0 || TargetHeight <= 0)
            errors.Add("Target sizes must be greater then 0.");

        return errorCount == errors.Count;
    }
}