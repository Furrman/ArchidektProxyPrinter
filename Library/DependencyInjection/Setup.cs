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
            .AddScoped<IArchidektApiClient, ArchidektApiClient>()
            .AddScoped<IScryfallApiClient, ScryfallApiClient>()
            // IO
            .AddScoped<ICardListFileParser, CardListFileParser>()
            .AddScoped<IFileManager, FileManager>()
            // Services
            .AddScoped<IArchidektService, ArchidektService>()
            .AddScoped<ILanguageService, LanguageService>()
            .AddScoped<IMagicCardService, MagicCardService>()
            .AddScoped<IWordGeneratorService, WordGeneratorService>()
        ;
    }
}