using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;

using Library.Models.DTO.Scryfall;
using Library.Models.DTO;

namespace Library.Clients;

public class ScryfallApiClient
{
    private readonly string _baseUrl = "https://api.scryfall.com";
    private readonly HttpClient _httpClient;
    private readonly ILogger<ScryfallApiClient> _logger;
    private readonly HttpClient _imageDownloadClient;

    public ScryfallApiClient(ILogger<ScryfallApiClient> logger)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl)
        };
        _imageDownloadClient = new HttpClient();
        _logger = logger;
    }


    public async Task<byte[]?> GetImage(string imageUrl)
    {
        try
        {
            return await _imageDownloadClient.GetByteArrayAsync(imageUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in downloading image from {imageUrl}", imageUrl);
            return null;
        }
    }

    public async Task<CardDataDTO?> FindCard(string cardName, string expansionCode, string collectorNumber, string? languageCode = null)
    {
        CardDataDTO? cardSearch = null;
        var searchSpecificCard = expansionCode != null && collectorNumber != null;
        if (!searchSpecificCard)
        {
            _logger.LogWarning("Missing ExpansionCode and CollectorNumber for the card '{cardName}'", cardName);
            return null;
        }
        var requestUrl = $"/cards/{expansionCode}/{collectorNumber}";
        if (languageCode is not null)
        {
            requestUrl += $"/{languageCode}";
        }

        try
        {
            var response = await _httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                cardSearch = await response.Content.ReadFromJsonAsync<CardDataDTO>();
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Card '{cardName}' not found", cardName);
            }
            else
            {
                _logger.LogError("Search request failed for card '{cardName}' - ({statusCode}) {reasonPhrase}", cardName, response.StatusCode, response.ReasonPhrase);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in searching for a card '{cardName}' images", cardName);
        }

        return cardSearch;
    }

    public async Task<CardSearchDTO?> SearchCard(string cardName, bool includeExtras, bool includeMultilingual)
    {
        CardSearchDTO? cardSearch = null;
        var requestUrl = $"/cards/search?q=${cardName}";
        if (includeExtras)
        {
            requestUrl += "&unique=prints&include_extras=true&include_variations=true";
        }
        if (includeMultilingual)
        {
            requestUrl += "&include_multilingual=true";
        }

        try
        {
            var response = await _httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                cardSearch = await response.Content.ReadFromJsonAsync<CardSearchDTO>();
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Card '{cardName}' not found", cardName);
            }
            else
            {
                _logger.LogError("Search request failed for card '{cardName}' - ({statusCode}) {reasonPhrase}", cardName, response.StatusCode, response.ReasonPhrase);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in searching for a card '{cardName}' images", cardName);
        }

        return cardSearch;
    }
}
