using Microsoft.Extensions.Logging;

namespace KingTech.Web.SimpleFileServer.UnitTests.Helpers;

public static class TestHelper
{
    /// <summary>
    /// Create a new ILogger instance.
    /// </summary>
    /// <typeparam name="TLogger">The ILogger subtype.</typeparam>
    /// <returns>A new ILogger instance for the given subtype.</returns>
    public static ILogger<TLogger> CreateLogger<TLogger>() => LoggerFactory.CreateLogger<TLogger>();

    /// <summary>
    /// UnitTestLogging logger factory.
    /// </summary>
    public static ILoggerFactory LoggerFactory => UnitTestLogging.NLoggerFactory;
}