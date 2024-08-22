using System.Collections.Immutable;
using KingTech.Web.SimpleFileServer.Abstract.Models;
using KingTech.Web.SimpleFileServer.Abstract.Transformers;

namespace KingTech.Web.SimpleFileServer.Services;

/// <summary>
/// This service is responsible for interacting with the registered <see cref="ITransformer"/> plugins.
/// </summary>
public interface ITransformerService
{
    /// <summary>
    /// Transform a StoredFile using the registered transformers.
    /// </summary>
    /// <param name="file">The file to transform.</param>
    /// <param name="arguments">The arguments passed in the original request, used for transforming the file.</param>
    public void Transform(StoredFile file, ImmutableDictionary<string, IEnumerable<string?>> arguments);
}