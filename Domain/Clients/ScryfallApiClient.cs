using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;

using Domain.Models.DTO.Scryfall;

namespace Domain.Clients;

/// <summary>
/// Represents an interface for interacting with the Scryfall API.
/// </summary>
public interface IScryfallApiClient
{
    /// <summary>
    /// Retrieves the image data for a given image URL.
    /// </summary>
    /// <param name="imageUrl">The URL of the image to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the image data as a byte array, or null if the image could not be retrieved.</returns>
    Task<byte[]?> DownloadImage(string imageUrl);

    /// <summary>
    /// Retrieves the card data for a specific card ID.
    /// </summary>
    /// <param name="cardId">The unique identifier of the card.</param>
    /// <returns>The card data for the specified card ID, or null if the card is not found.</returns>
    Task<CardDataDTO?> GetCard(Guid cardId);

    /// <summary>
    /// Finds a card by its name, expansion code, collector number, and optional language code.
    /// </summary>
    /// <param name="cardName">The name of the card to find.</param>
    /// <param name="expansionCode">The code of the expansion the card belongs to.</param>
    /// <param name="collectorNumber">The collector number of the card within the expansion.</param>
    /// <param name="languageCode">The optional language code of the card.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the card data as a CardDataDTO object, or null if the card could not be found.</returns>
    Task<CardDataDTO?> FindCard(string cardName, string expansionCode, string collectorNumber, string? languageCode = null);

    /// <summary>
    /// Searches for cards by their name, with options to include extras and multilingual versions.
    /// </summary>
    /// <param name="cardName">The name of the card to search for.</param>
    /// <param name="includeExtras">A flag indicating whether to include extras (e.g., tokens, promos) in the search results.</param>
    /// <param name="includeMultilingual">A flag indicating whether to include multilingual versions of the card in the search results.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the search results as a CardSearchDTO object, or null if no cards match the search criteria.</returns>
    Task<CardSearchDTO?> SearchCard(string cardName, bool includeExtras, bool includeMultilingual);
}

public class ScryfallApiClient : IScryfallApiClient
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


    public async Task<byte[]?> DownloadImage(string imageUrl)
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

    public async Task<CardDataDTO?> GetCard(Guid cardId)
    {
        CardDataDTO? cardData = null;
        var requestUrl = $"/cards/{cardId}";

        try
        {
            var response = await _httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                cardData = await response.Content.ReadFromJsonAsync<CardDataDTO>();
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Card with ID '{cardId}' not found", cardId);
            }
            else
            {
                _logger.LogError("Request failed for card with ID '{cardId}' - ({statusCode}) {reasonPhrase}", cardId, response.StatusCode, response.ReasonPhrase);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in retrieving card with ID '{cardId}'", cardId);
        }

        return cardData;
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
