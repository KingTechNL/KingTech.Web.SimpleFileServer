using System.Collections.Immutable;
using System.Text;
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
    private const string WidthArgumentKey = "width";
    private const string HeightArgumentKey = "height";
    private const string KeepAspectRatioArgumentKey = "keepaspectratio";
    private const string ResizeModeArgumentKey = "resizemode";

    /// <inheritdoc cref="ITransformer"/>
    public string Name { get; } = "ImageResize";

    private readonly ILogger<ImageResizeTransformer> _logger;
    private readonly ImageResizeTransformerSettings _settings;

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
    /// <param name="transformerKey">Key for the query parameter that is passed when requesting this file.</param>
    /// <param name="storedFile">StoredFile as retrieved from the file source.</param>
    /// <returns>True if transformer should be applied to the file, false otherwise.</returns>
    public bool Match(string transformerKey, StoredFile storedFile)
    {
        //Check if file is available.
        if (storedFile == null || string.IsNullOrWhiteSpace(transformerKey))
            return false;
        //Check if argument matches supported arguments.
        if(transformerKey.ToLower() != "resize" && transformerKey.ToLower() != "thumb")
            return false;
        //Check if we can transform this file.
        if (GetEncoder(storedFile.Extension) == null)
            return false;
        return true;
    }

    /// <summary>
    /// Resize the given image file based on the target size set in the settings.
    /// </summary>
    /// <param name="storedFile">StoredFile as retrieved from the file source.</param>
    /// <param name="transformerKey">Key of the query parameter used to trigger the transformer.</param>
    /// <param name="arguments">Immutable list of arguments passed in the query parameters of the original request.</param>
    /// <returns>True on success, false otherwise.</returns>
    public bool Transform(StoredFile storedFile, string transformerKey, ImmutableDictionary<string, IEnumerable<string?>> arguments)
    {
        switch (transformerKey.ToLower())
        {
            case "resize":
                var resizeValues = GetResizeValues(arguments);
                return Resize(storedFile, resizeValues.Width, resizeValues.Height, resizeValues.resizeMode);
            case "thumb":
                return Resize(storedFile, _settings.ThumbnailWidth, _settings.ThumbnailHeight,
                    _settings.KeepThumbnailAspectRatio ? ResizeMode.Pad : ResizeMode.Crop);
            default:
                return false;
        }
    }

    /// <summary>
    /// Resize the given image file based on the given dimensions and mode.
    /// </summary>
    /// <param name="imageFile">The file to resize.</param>
    /// <param name="width">The new width of the file.</param>
    /// <param name="height">The new height of the file.</param>
    /// <param name="mode">The resize mode to apply.</param>
    /// <returns>True on success, false otherwise.</returns>
    private bool Resize(StoredFile imageFile, int? width, int? height, ResizeMode mode = ResizeMode.Crop)
    {
        //Get encoder for file.
        var encoder = GetEncoder(imageFile.Extension);
        if (encoder == null)
        {
            _logger.LogError("Cannot resize {extension} files.", imageFile.Extension);
            return false;
        }

        try
        {
            //Load image.
            var image = SixLabors.ImageSharp.Image.Load(imageFile.File);

            _logger.LogInformation("Resizing {fileName} to {w}x{h} (resize mode: {mode})",
                imageFile.Name, width ?? image.Width, height ?? image.Height, mode.ToString());

            //Resize image.
            image.Mutate(m => m.Resize(new ResizeOptions()
            {
                Size = new SixLabors.ImageSharp.Size(width ?? image.Width, height ?? image.Height),
                Mode = mode
            }));
            var resizedImage = new MemoryStream();
            image.Save(resizedImage, encoder);

            //Replace existing (in memory) file.
            imageFile.File = resizedImage;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resize image.");
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

    /// <summary>
    /// Get the resize values from the passed arguments.
    /// </summary>
    /// <param name="arguments">The query parameters passed in the original request.</param>
    /// <returns>The width, height and resize mode values to use for resizing.</returns>
    private (int? Width, int? Height, ResizeMode resizeMode) GetResizeValues(ImmutableDictionary<string, IEnumerable<string?>> arguments)
    {
        int? width = null;
        int? height = null;
        var mode = ResizeMode.Crop;

        //Try get width from arguments.
        if (arguments.TryGetValue(WidthArgumentKey, out var widthArgument))
        {
            if(widthArgument.Count() > 1)
                _logger.LogWarning("Multiple {argumentKey} arguments found. Using first.", WidthArgumentKey);
            if (int.TryParse(widthArgument.First(), out var parsedWidth))
            {
                if (parsedWidth <= 0)
                {
                    _logger.LogWarning("Unable to resize to width < 0!");
                }
                else
                {
                    width = parsedWidth;
                }
            }
            else
            {
                _logger.LogError("Failed to parse width ({width}) to integer.", widthArgument);
            }
        }
        else
        {
            _logger.LogDebug("No width argument found.");
        }

        //Try get height from arguments.
        if (arguments.TryGetValue(HeightArgumentKey, out var heightArgument))
        {
            if (heightArgument.Count() > 1)
                _logger.LogWarning("Multiple {argumentKey} arguments found. Using first.", HeightArgumentKey);
            if (int.TryParse(heightArgument.First(), out var parsedHeight))
            {
                if (parsedHeight <= 0)
                {
                    _logger.LogWarning("Unable to resize to height < 0!");
                }
                else
                {
                    height = parsedHeight;
                }
            }
            else
            {
                _logger.LogError("Failed to parse height ({height}) to integer.", heightArgument);
            }
        }
        else
        {
            _logger.LogDebug("No height argument found.");
        }

        //try get 'keep aspect ratio' from arguments.
        if (arguments.TryGetValue(KeepAspectRatioArgumentKey, out var keepAspectRatioArgument))
        {
            if (keepAspectRatioArgument.Count() > 1)
                _logger.LogWarning("Multiple {argumentKey} arguments found. Using first.", KeepAspectRatioArgumentKey);
            if (keepAspectRatioArgument.FirstOrDefault() == "true")
            {
                mode = ResizeMode.Min;
            }
        }
        else if (arguments.TryGetValue(ResizeModeArgumentKey, out var resizeModeArgument))
        {
            if (resizeModeArgument.Count() > 1)
                _logger.LogWarning("Multiple {argumentKey} arguments found. Using first.", ResizeModeArgumentKey);

            switch (resizeModeArgument.First())
            {
                case "min":
                    mode = ResizeMode.Min;
                    break;
                case "max":
                    mode = ResizeMode.Max; 
                    break;
                case "crop":
                    mode = ResizeMode.Crop;
                    break;
                case "pad":
                    mode = ResizeMode.Pad;
                    break;
                case "boxpad":
                    mode = ResizeMode.BoxPad;
                    break;
                case "stretch":
                    mode = ResizeMode.Stretch;
                    break;
                default:
                    break;
            }
        }

        return (width, height, mode);
    }
}
