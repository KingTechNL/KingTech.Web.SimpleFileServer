using System.Collections.Immutable;
using KingTech.Web.SimpleFileServer.Abstract.Models;
using KingTech.Web.SimpleFileServer.Abstract.Transformers;

namespace KingTech.Web.SimpleFileServer.Services;

/// <summary>
/// This service is responsible for interacting with the registered <see cref="ITransformer"/> plugins.
/// </summary>
public class TransformerService : ITransformerService
{
    private const string TransformerParameterKey = "transform";

    private readonly IEnumerable<ITransformer> _transformers;
    private readonly ILogger<TransformerService> _logger;

    /// <summary>
    /// This service is responsible for interacting with the registered <see cref="ITransformer"/> plugins.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> responsible for logging from this class.</param>
    /// <param name="transformers">A list of registered <see cref="ITransformer"/> plugins.</param>
    public TransformerService(ILogger<TransformerService> logger, IEnumerable<ITransformer> transformers)
    {
        _logger = logger;
        _transformers = transformers;
    }

    /// <summary>
    /// Transform a StoredFile using the registered transformers.
    /// </summary>
    /// <param name="file">The file to transform.</param>
    /// <param name="arguments">The arguments passed in the original request, used for transforming the file.</param>
    public void Transform(StoredFile file, ImmutableDictionary<string, IEnumerable<string?>> arguments)
    {
        //Get all requested transformers.
        if (!arguments.TryGetValue(TransformerParameterKey, out var transformers))
            return; //No transformation required.

        foreach (var requestedTransformer in transformers)
        {
            foreach (var transformer in _transformers)
            {
                if (transformer.Match(requestedTransformer, file) && !transformer.Transform(file, requestedTransformer, arguments))
                {
                    _logger.LogWarning("{transformer} failed to transform file {@File}", transformer.GetType().Name, file);
                }
            }
        }

        //TODO: This seems to cause problems with 'web' streams.
        file.File.Position = 0; //Some transformers might leave position somewhere else.
    }
}