using Microsoft.Extensions.DependencyInjection;

using Domain.Clients;

namespace Domain.DependencyInjection;

public static class HttpClientFactorySetup
{
    public static IServiceCollection ConfigureHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<ArchidektClient>(client =>
        {
            client.BaseAddress = new Uri("https://archidekt.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddHttpClient<ScryfallApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.scryfall.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}