using NLog.Config;
using NLog.Extensions.Logging;

namespace KingTech.Web.SimpleFileServer.UnitTests.Helpers;

public static class UnitTestLogging
{
    private static NLogLoggerFactory _nLogLoggerFactory;
    private static string _configFile = "NLog.Testing.config";

    public static NLogLoggerFactory NLoggerFactory
    {
        get
        {
            if (_nLogLoggerFactory == null)
            {
                _nLogLoggerFactory = new NLogLoggerFactory();
                if (File.Exists(_configFile))
                {
                    NLog.LogManager.Configuration = new XmlLoggingConfiguration(_configFile);
                }
                else
                {
                    Console.WriteLine($"Could not fine {_configFile}");
                }
            }

            return _nLogLoggerFactory;
        }

    }
}