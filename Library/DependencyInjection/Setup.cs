using Library.Clients;
using Library.IO;
using Library.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Library.DependencyInjection;

public static class Setup
{
    public static IServiceCollection SetupLibraryClasses(this IServiceCollection services)
    {
        return services
            .AddScoped<ArchidektPrinter>()
            .AddScoped<ArchidektApiClient>()
            .AddScoped<ScryfallApiClient>()
            .AddScoped<CardListFileParser>()
            .AddScoped<MagicCardService>()
            .AddScoped<WordGeneratorService>()
            .AddScoped<FileManager>()
        ;
    }
}