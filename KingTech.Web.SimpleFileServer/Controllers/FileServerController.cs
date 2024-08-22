using System.Collections.Immutable;
using System.ComponentModel.Design;
using System.Text.Encodings.Web;
using System.Web;
using KingTech.Web.SimpleFileServer.Abstract.Models;
using KingTech.Web.SimpleFileServer.Abstract.Sources;
using KingTech.Web.SimpleFileServer.Abstract.Transformers;
using KingTech.Web.SimpleFileServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace KingTech.Web.SimpleFileServer.Controllers
{
    [ApiController]
    [Route("/")]
    public class FileServerController : ControllerBase
    {
        private readonly ILogger<FileServerController> _logger;
        private readonly GeneralSettings _generalSettings;
        private readonly IFileSourceService _fileSourceService;
        private readonly ITransformerService _transformerService;

        public FileServerController(ILogger<FileServerController> logger, GeneralSettings generalSettings, 
            IFileSourceService fileSourceService, ITransformerService transformerService)
        {
            _logger = logger;
            _generalSettings = generalSettings;
            _fileSourceService = fileSourceService;
            _transformerService = transformerService;
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
        /// <param name="fileName">The name of the file to fetch, including its path from the base directory.</param>
        /// <returns>The specified file from the server as a file-stream.</returns>
        [HttpGet("{fileName}")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFile(string fileName)
        {
            //Check parameters
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("Invalid file name passed");

            //Get file from source.
            fileName = HttpUtility.UrlDecode(fileName);
            var file = await _fileSourceService.GetFile(fileName);
            
            if (file == null)
            {
                _logger.LogError("No file found for '{file}'.", fileName);
                return BadRequest($"No file found for '{fileName}'.");
            }

            //Transform file if needed.
            var args = HttpContext.Request.Query.ToImmutableDictionary(
                k => k.Key,
                v => (IEnumerable<string>) v.Value.ToList());
            _transformerService.Transform(file, args);

            //Determine the Content Type of the File.
            var contentTypeFound = new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType);
            if (!contentTypeFound || string.IsNullOrWhiteSpace(contentType))
            {
                _logger.LogError("No content type found for {file} ({cleanFileName})", file, fileName);
                return BadRequest($"No content type found for {fileName} ({fileName})");
            }

            return new FileStreamResult(file.File, contentType); //TODO: Return clear error status codes on exceptions.
        }

        /// <summary>
        /// Get a list of files from the base directory.
        /// </summary>
        /// <returns>A list of file names.</returns>
        [HttpGet("list")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListFiles() => await ListFiles(null);

        /// <summary>
        /// Get a list of files from the given directory.
        /// </summary>
        /// <param name="directory">The directory path (from the base-directory) to list all files in.</param>
        /// <returns>A list of file names.</returns>
        [HttpGet("list/{directory}")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListFiles(string? directory)
        {
            //Convert url encoded string back to normal string.
            if(!string.IsNullOrEmpty(directory))
                directory = HttpUtility.UrlDecode(directory);

            //Get list of files from all sources.
            var files = await _fileSourceService.GetFiles(directory);

            return Ok(files); //TODO: Return clear error status codes on exceptions.
        }


        /// <summary>
        /// Get a list of sub-directories in the base-directory.
        /// </summary>
        /// <returns>A list of directory names.</returns>
        [HttpGet("directories")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListDirectories() => await ListDirectories(null);

        /// <summary>
        /// Get a list of sub-directories in the given directory path.
        /// </summary>
        /// <param name="directory">the directory path to get sub-directories for based on the base-directory.</param>
        /// <returns>A list of directory names.</returns>
        [HttpGet("directories/{directory}")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListDirectories(string? directory)
        {
            //Convert url encoded string back to normal string.
            if (!string.IsNullOrEmpty(directory))
                directory = HttpUtility.UrlDecode(directory);

            //Get list of (sub) directories from all sources.
            var directories = await _fileSourceService.GetDirectories(directory);

            return Ok(directories); //TODO: Return clear error status codes on exceptions.
        }

    }
}