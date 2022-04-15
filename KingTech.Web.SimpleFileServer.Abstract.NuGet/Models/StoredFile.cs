namespace KingTech.Web.SimpleFileServer.Abstract.Models
{
    public class StoredFile : IDisposable
    {
        /// <summary>
        /// The location this file is stored.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Stream containing the actual file.
        /// </summary>
        public Stream File
        {
            get => _file;
            set
            {
                _file?.Close();
                _file = value;
            }
        }
        private Stream _file;

        /// <summary>
        /// The (real) name of this file.
        /// </summary>
        public string Name => Path.GetFileName(Location);

        /// <summary>
        /// Extension for this file.
        /// </summary>
        public string Extension => Path.GetExtension(Location).ToLower();

        /// <summary>
        /// StoredFile object used to pass file through the service.
        /// </summary>
        /// <param name="fileLocation">The location the original file is stored.</param>
        /// <param name="fileStream">Stream containing the file.</param>
        public StoredFile(string fileLocation, Stream fileStream)
        {
            if(string.IsNullOrWhiteSpace(fileLocation))
                throw new ArgumentNullException(nameof(fileLocation));
            Location = fileLocation;
            File = fileStream ?? throw new ArgumentNullException(nameof(fileStream));
        }

        /// <summary>
        /// Called when this class is deconstructed.
        /// Makes sure the (file) stream is closed.
        /// </summary>
        public void Dispose()
        {
            _file?.Close();
            _file?.Dispose();
        }
    }
}
