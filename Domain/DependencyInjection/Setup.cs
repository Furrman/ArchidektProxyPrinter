using Microsoft.Extensions.DependencyInjection;

using Domain.Clients;
using Domain.Factories;
using Domain.IO;
using Domain.Services;

namespace Domain.DependencyInjection;

public static class Setup
{
    public static IServiceCollection SetupLibraryClasses(this IServiceCollection services)
    {
        return services
            .AddScoped<IMagicProxyPrinter, MagicProxyPrinter>()
            // Clients
            .AddScoped<IArchidektApiClient, ArchidektApiClient>()
            .AddScoped<IScryfallApiClient, ScryfallApiClient>()
            // Factories
            .AddScoped<IDeckRetrieverFactory, DeckRetrieverFactory>()
            // IO
            .AddScoped<ICardListFileParser, CardListFileParser>()
            .AddScoped<IFileManager, FileManager>()
            .AddScoped<IWordDocumentWrapper, WordDocumentWrapper>()
            // Services
            .AddScoped<IArchidektService, ArchidektService>()
            .AddScoped<IScryfallService, ScryfallService>()
            .AddScoped<ILanguageService, LanguageService>()
            .AddScoped<IWordGeneratorService, WordGeneratorService>()
        ;
    }
}