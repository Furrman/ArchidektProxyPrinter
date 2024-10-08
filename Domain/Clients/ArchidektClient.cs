using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

using Domain.Models.DTO.Archidekt;

namespace Domain.Clients;

/// <summary>
/// Represents an interface for interacting with the Archidekt API.
/// </summary>
public interface IArchidektClient
{
    /// <summary>
    /// Retrieves a deck from the Archidekt API based on the specified deck ID.
    /// </summary>
    /// <param name="deckId">The ID of the deck to retrieve.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the retrieved <see cref="DeckDTO"/> if successful, or <c>null</c> if the deck was not found.</returns>
    Task<DeckDTO?> GetDeck(int deckId);
}

public class ArchidektClient(HttpClient httpClient, ILogger<ArchidektClient> logger) : IArchidektClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ILogger<ArchidektClient> _logger = logger;

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
