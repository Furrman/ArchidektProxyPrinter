using System.Reflection;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;

namespace ConsoleApp.Configuration;

internal static class NLogConfigurator
{
    public static IServiceCollection SetupNLog(this IServiceCollection serviceCollection)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("nlog.config"));

        string configContent = string.Empty;
        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is not null)
        {
            using StreamReader reader = new(stream!);
            configContent = reader.ReadToEnd();
        }

        var nlogConfig = new XmlLoggingConfiguration(XmlReader.Create(new StringReader(configContent)));

        return serviceCollection
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Warning);
                builder.AddNLog(nlogConfig, new NLogProviderOptions
                {
                    CaptureMessageProperties = true,
                    CaptureMessageTemplates = true
                });
            });
    }
}
