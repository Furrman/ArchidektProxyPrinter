using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog;
using NLog.Extensions.Logging;

namespace ConsoleApp.Configuration;

internal static class NLogConfigurator
{
    public static IServiceCollection SetupNLog(this IServiceCollection serviceCollection)
    {
        string executablePath = AppDomain.CurrentDomain.BaseDirectory;
        string configFilePath = Path.Combine(executablePath, "nlog.config");
        LogManager.Configuration = new XmlLoggingConfiguration(configFilePath);
        return serviceCollection
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                builder.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageProperties = true,
                    CaptureMessageTemplates = true
                });
            });
    }
}
