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
            .AddScoped<IArchidektPrinter, ArchidektPrinter>()
            // Clients
            .AddScoped<ArchidektApiClient>()
            .AddScoped<ScryfallApiClient>()
            // IO
            .AddScoped<CardListFileParser>()
            .AddScoped<IFileManager, FileManager>()
            // Services
            .AddScoped<IArchidektService, ArchidektService>()
            .AddScoped<ILanguageService, LanguageService>()
            .AddScoped<IMagicCardService, MagicCardService>()
            .AddScoped<WordGeneratorService>()
        ;
    }
}