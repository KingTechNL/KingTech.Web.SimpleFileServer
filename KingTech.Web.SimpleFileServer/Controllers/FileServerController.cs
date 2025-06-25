using System.Collections.Immutable;
using System.Text.Encodings.Web;
using System.Web;
using KingTech.Web.SimpleFileServer.Abstract.Models;
using KingTech.Web.SimpleFileServer.Abstract.Sources;
using KingTech.Web.SimpleFileServer.Abstract.Transformers;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace KingTech.Web.SimpleFileServer.Controllers
{
    [ApiController]
    [Route("/")]
    public class FileServerController : ControllerBase
    {
        private const string TransformerParameterKey = "transform";

        private readonly ILogger<FileServerController> _logger;
        private readonly GeneralSettings _generalSettings;
        private readonly IEnumerable<ITransformer> _transformers;
        private readonly IEnumerable<IFileSource> _sources;

        public FileServerController(ILogger<FileServerController> logger, GeneralSettings generalSettings,
            IEnumerable<ITransformer> transformers, IEnumerable<IFileSource> sources)
        {
            _logger = logger;
            _generalSettings = generalSettings;
            _transformers = transformers;
            _sources = sources;
        }

        /// <summary>
        /// Return a short text describing this service.
        /// </summary>
        /// <returns>A short text describing this service.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index() => Ok(_generalSettings?.ServiceDescription ?? string.Empty);

        /// <summary>
        /// Get a specific file from the file server.
        /// </summary>
        /// <param name="filePath">The name of the file to fetch, including its path from the base directory.</param>
        /// <returns>The specified file from the server as a file-stream.</returns>
        [HttpGet("file/{*filePath}")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetFile(string filePath)
        {
            //Check parameters
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest("Invalid file name passed");

            //Get file from source.
            filePath = HttpUtility.UrlDecode(filePath);
            var file = LoadFromSource(filePath);
            
            if (file == null)
            {
                _logger.LogError("No file found for '{file}'.", filePath);
                return BadRequest($"No file found for '{filePath}'.");
            }

            //Transform file if needed.
            var args = HttpContext.Request.Query.ToImmutableDictionary(
                k => k.Key,
                v => (IEnumerable<string>) v.Value.ToList());
            Transform(file, args);

            //Determine the Content Type of the File.
            var contentTypeFound = new FileExtensionContentTypeProvider().TryGetContentType(filePath, out var contentType);
            if (!contentTypeFound || string.IsNullOrWhiteSpace(contentType))
            {
                _logger.LogError("No content type found for {file} ({cleanFileName})", file, filePath);
                return BadRequest($"No content type found for {filePath} ({filePath})");
            }

            file.File.Position = 0; //Some transformers might leave position somewhere else. TODO: Does this need to be in the transform loop?
            var result = new FileStreamResult(file.File, contentType);
            
            return result;
        }

        /// <summary>
        /// Get a list of files from the base directory.
        /// </summary>
        /// <returns>A list of file names.</returns>
        [HttpGet("list")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public IActionResult ListFiles() => ListFiles(null);

        /// <summary>
        /// Get a list of files from the given directory.
        /// </summary>
        /// <param name="directory">The directory path (from the base-directory) to list all files in.</param>
        /// <returns>A list of file names.</returns>
        [HttpGet("list/{*directory}")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public IActionResult ListFiles(string? directory)
        {
            //Convert url encoded string back to normal string.
            if(!string.IsNullOrEmpty(directory))
                directory = HttpUtility.UrlDecode(directory);

            //Get list of files from all sources.
            var files = new List<string>();
            foreach (var source in _sources)
            {
                var sourceFiles = source.ListFiles(directory);
                if(sourceFiles != null && sourceFiles.Any())
                    files.AddRange(sourceFiles);
            }

            return Ok(files);
        }


        /// <summary>
        /// Get a list of sub-directories in the base-directory.
        /// </summary>
        /// <returns>A list of directory names.</returns>
        [HttpGet("directories")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public IActionResult ListDirectories() => ListDirectories(null);

        /// <summary>
        /// Get a list of sub-directories in the given directory path.
        /// </summary>
        /// <param name="directory">the directory path to get sub-directories for based on the base-directory.</param>
        /// <returns>A list of directory names.</returns>
        [HttpGet("directories/{*directory}")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public IActionResult ListDirectories(string? directory)
        {
            //Convert url encoded string back to normal string.
            if (!string.IsNullOrEmpty(directory))
                directory = HttpUtility.UrlDecode(directory);

            //Get list of (sub) directories from all sources.
            var directories = new List<string>();
            foreach (var source in _sources)
            {
                var sourceDirectories = source.ListDirectories(directory);
                if (sourceDirectories != null && sourceDirectories.Any())
                    directories.AddRange(sourceDirectories);
            }

            return Ok(directories);
        }

        /// <summary>
        /// Load the file using the registered filesources.
        /// </summary>
        /// <param name="fileName">Filename (minus the postfix for transformers).</param>
        /// <returns>The loaded file including metadata, or null if no such file was found.</returns>
        private StoredFile LoadFromSource(string fileName)
        {
            StoredFile file = null;
            foreach (var source in _sources)
            {
                file = source.GetFile(fileName);
                if (file != null)
                    break;
            }

            return file;
        }

        /// <summary>
        /// Transform a StoredFile using the registered transformers.
        /// </summary>
        /// <param name="file">The file to transform.</param>
        /// <param name="arguments">The arguments passed in the original request, used for tranforming the file.</param>
        private void Transform(StoredFile file, ImmutableDictionary<string, IEnumerable<string?>> arguments)
        {
            //Get all requested transformers.
            if(!arguments.TryGetValue(TransformerParameterKey, out var transformers))
                   return; //No transformation required.

            foreach (var requestedTransformer in transformers)
            {
                foreach (var transformer in _transformers)
                {
                    if(transformer.Match(requestedTransformer, file) && !transformer.Transform(file, requestedTransformer, arguments))
                    {
                        _logger.LogWarning("{transformer} failed to transform file {@File}", transformer.GetType().Name, file);
                    }
                }
            }
        }
    }
}