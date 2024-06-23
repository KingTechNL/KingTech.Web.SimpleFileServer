using System.Collections.Immutable;
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
        private const string TransformerParameterKey = "transform";

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
            var file = LoadFromSource(fileName);
            
            if (file == null)
            {
                _logger.LogError("No file found for '{file}'.", fileName);
                return BadRequest($"No file found for '{fileName}'.");
            }

            //Transform file if needed.
            var args = HttpContext.Request.Query.ToImmutableDictionary(
                k => k.Key,
                v => (IEnumerable<string>) v.Value.ToList());
            Transform(file, args);

            //Determine the Content Type of the File.
            var contentTypeFound = new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType);
            if (!contentTypeFound || string.IsNullOrWhiteSpace(contentType))
            {
                _logger.LogError("No content type found for {file} ({cleanFileName})", file, fileName);
                return BadRequest($"No content type found for {fileName} ({fileName})");
            }

            file.File.Position = 0; //Some transformers might leave position somewhere else. TODO: Does this need to be in the transform loop?
            var result = new FileStreamResult(file.File, contentType);
            
            return result;
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