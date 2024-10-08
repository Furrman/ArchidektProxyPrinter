using Microsoft.Extensions.DependencyInjection;

using Domain.Clients;
using Domain.IO;
using Domain.Services;

namespace Domain.DependencyInjection;

public static class Setup
{
    public static IServiceCollection SetupLibraryClasses(this IServiceCollection services)
    {
        return services
            .AddScoped<IMTGProxyPrinter, MTGProxyPrinter>()
            // Clients
            .AddScoped<IArchidektApiClient, ArchidektApiClient>()
            .AddScoped<IScryfallApiClient, ScryfallApiClient>()
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