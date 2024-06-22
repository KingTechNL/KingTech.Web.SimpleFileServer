using System.Collections.Immutable;
using KingTech.Web.SimpleFileServer.Abstract.Models;

namespace KingTech.Web.SimpleFileServer.Abstract.Transformers;

/// <summary>
/// Transformers are used to modify files if needed.
/// Multiple transformers can be applied.
/// </summary>
public interface ITransformer
{
    /// <summary>
    /// The unique name of this transformer. Used to determine whether the transformer should be loaded / used or not.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Check whether or not this transformer should be applied on the file.
    /// </summary>
    /// <param name="transformerKey">Key of the query parameter used to trigger the transformer.</param>
    /// <param name="storedFile">StoredFile as retrieved from the file source.</param>
    /// <returns>True if transformer should be applied to the file, false otherwise.</returns>
    public bool Match(string transformerKey, StoredFile storedFile);

    /// <summary>
    /// Modify the given file (e.g. resize images, change text, ...).
    /// Modification depends completely on the implementation.
    /// </summary>
    /// <param name="storedFile">StoredFile as retrieved from the file source.</param>
    /// <param name="transformerKey">Key of the query parameter used to trigger the transformer.</param>
    /// <param name="arguments">Immutable list of arguments passed in the query parameters of the original request.</param>
    /// <returns>Stream containing the modified file.</returns>
    public bool Transform(StoredFile storedFile, string transformerKey, ImmutableDictionary<string, IEnumerable<string?>> arguments);
}