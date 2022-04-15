using KingTech.Web.SimpleFileServer.Abstract.Models;
using KingTech.Web.SimpleFileServer.Abstract.Sources;
using KingTech.Web.SimpleFileServer.Abstract.Transformers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace KingTech.Web.SimpleFileServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FileServerController : ControllerBase
    {
        

        private readonly ILogger<FileServerController> _logger;
        private readonly IEnumerable<ITransformer> _transformers;
        private readonly IEnumerable<IFileSource> _sources;

        public FileServerController(ILogger<FileServerController> logger, IEnumerable<ITransformer> transformers, IEnumerable<IFileSource> sources)
        {
            _logger = logger;
            _transformers = transformers;
            _sources = sources;
        }

        [HttpGet("{fileName}")]
        public IActionResult Get(string fileName)
        {
            //Check parameters
            if (string.IsNullOrWhiteSpace(fileName))
                return BadRequest("Invalid file name passed");

            //Get file from source.
            var cleanFileName = StripPostFixes(fileName);
            var file = LoadFromSource(cleanFileName);
            
            if (file == null)
            {
                _logger.LogError("No file found for {file} ({cleanFileName})", fileName, cleanFileName);
                return BadRequest($"No file found for {fileName} ({cleanFileName})");
            }

            //Transform file if needed.
            TransForm(fileName, file);

            //Determine the Content Type of the File.
            var contentTypeFound = new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType);
            if (!contentTypeFound || string.IsNullOrWhiteSpace(contentType))
            {
                _logger.LogError("No content type found for {file} ({cleanFileName})", file, cleanFileName);
                return BadRequest($"No content type found for {fileName} ({cleanFileName})");
            }

            file.File.Position = 0; //Some transformers might leave position somewhere else.
            var result = new FileStreamResult(file.File, contentType);
            
            return result;
        }

        /// <summary>
        /// Strip all transformer logic from the received filename.
        /// </summary>
        /// <param name="fileName">The filename as received from the web request.</param>
        /// <returns>The fileName stripped from all transformer logic.</returns>
        private string StripPostFixes(string fileName)
        {
            foreach (var transformer in _transformers)
            {
                fileName = transformer.GetCleanFileName(fileName);
            }

            return fileName;
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
        /// <param name="fileName">The original name of the file as called by the used.</param>
        /// <param name="file">The file to transform.</param>
        private void TransForm(string fileName, StoredFile file)
        {
            foreach (var transformer in _transformers)
            {
                if (transformer.Match(fileName, file) && !transformer.Transform(fileName, file))
                {
                    _logger.LogWarning("{transformer} failed to transform file {@File}", transformer.GetType().Name, file);
                }
            }
        }
    }
}