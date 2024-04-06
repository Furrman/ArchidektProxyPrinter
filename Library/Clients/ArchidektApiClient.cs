using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

using Library.Models.DTO.Archidekt;

namespace Library.Clients;

public class ArchidektApiClient
{
    private readonly string _baseUrl = "https://archidekt.com/";
    private readonly HttpClient _httpClient;
    private readonly ILogger<ArchidektApiClient> _logger;

    public ArchidektApiClient(ILogger<ArchidektApiClient> logger)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl)
        };
        _logger = logger;
    }

    public async Task<DeckDTO?> GetDeck(int deckId)
    {
        DeckDTO? deckDto = null;
        var requestUrl = $"/api/decks/{deckId}/";
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(requestUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeckId: {deckId} Error in getting card list from the deck", deckId);
            return deckDto;
        }

        if (response.IsSuccessStatusCode)
        {
            try
            {
                deckDto = await response.Content.ReadFromJsonAsync<DeckDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeckId: {deckId} Error in parsing card list from the deck", deckId);
                return deckDto;
            }
        }
        else
        {
            _logger.LogWarning("DeckId: {deckId} Failure response from getting card list from the deck Request: {statusCode} {reasonPhrase}", deckId, response.StatusCode, response.ReasonPhrase);
        }

        return deckDto;
    }
}
