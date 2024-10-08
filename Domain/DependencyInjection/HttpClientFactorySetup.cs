using Microsoft.Extensions.DependencyInjection;

using Polly;
using Polly.Extensions.Http;

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
        })
        .AddPolicyHandler(GetRetryPolicy());
        services.AddHttpClient<ScryfallApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.scryfall.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddPolicyHandler(GetRetryPolicy());

        return services;
    }
    
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()  // Handles 5xx, 408 and other transient errors
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests) // Retry on 429 (Too Many Requests)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));  // Exponential backoff
    }
}