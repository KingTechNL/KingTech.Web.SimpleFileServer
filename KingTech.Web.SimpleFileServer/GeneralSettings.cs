namespace KingTech.Web.SimpleFileServer;

public class GeneralSettings
{
    /// <summary>
    /// A description that is given when accessing the default route (http://[host]:[port]) of this service.
    /// </summary>
    public string ServiceDescription { get; set; } = "This server runs a SimpleFileServer. For more information, visit https://github.com/KingTechNL/KingTech.Web.SimpleFileServer";

    /// <summary>
    /// If set to true, the file paths in the HTTP(s) requests are expected to start with a source name.
    /// E.g.  http://[host]:[port]/[sourceName]/[filepath]
    ///
    /// If set to false and multiple sources are configured, we can combine them all into one (all files/folders show in the same list).
    /// Warning: This may cause problems with duplicate paths.  (e.g.  /mydir/myfile.txt existing in multiple file sources).
    /// </summary>
    public bool PrefixSourceName { get; set; } = false;
}