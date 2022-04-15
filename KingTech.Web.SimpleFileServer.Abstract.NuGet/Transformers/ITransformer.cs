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
    /// <param name="fileName">Name of the file, as requested by the endpoint.</param>
    /// <param name="storedFile">StoredFile as retrieved from the file source.</param>
    /// <returns>True if transformer should be applied to the file, false otherwise.</returns>
    public bool Match(string fileName, StoredFile storedFile);

    /// <summary>
    /// Clean up a filename as received from the user-endpoint based on the postfix or other identifiers needed by this transformer.
    /// </summary>
    /// <param name="fileName">The filename to clean up.</param>
    /// <returns>The filename trimmed from all information needed for this transformer.</returns>
    public string GetCleanFileName(string fileName);

    /// <summary>
    /// Modify the given file (e.g. resize images, change text, ...).
    /// Modification depends completely on the implementation.
    /// </summary>
    /// <param name="fileName">Name of the file, as requested by the endpoint.</param>
    /// <param name="storedFile">StoredFile as retrieved from the file source.</param>
    /// <returns>Stream containing the modified file.</returns>
    public bool Transform(string fileName, StoredFile storedFile);
}