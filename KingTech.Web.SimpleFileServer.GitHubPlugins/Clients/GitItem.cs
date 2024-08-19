namespace KingTech.Web.SimpleFileServer.GitHubPlugins.Clients;

public record GitItem
{
    public string Name { get; set; }

    public ItemType Type { get; set; }
    
    public Stream? fileStream { get; set; }

    public string? Url { get; set; }
}

public enum ItemType
{
    File,
    Directory
}