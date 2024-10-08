using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

using Domain.Clients;
using Domain.Factories;
using Domain.Models.DTO;
using Domain.Models.DTO.Archidekt;

namespace Domain.Services;

/// <summary>
/// Represents a service for interacting with Archidekt.
/// </summary>
public interface IArchidektService : IDeckRetriever
{
    /// <summary>
    /// Tries to extract the deck ID from the given URL.
    /// </summary>
    /// <param name="url">The URL to extract the deck ID from.</param>
    /// <param name="deckId">The extracted deck ID, if successful.</param>
    /// <returns><c>true</c> if the deck ID was successfully extracted; otherwise, <c>false</c>.</returns>
    bool TryExtractDeckIdFromUrl(string url, out int deckId);
}

public class ArchidektService(IArchidektApiClient archidektApiClient, ILogger<ArchidektService> logger) : IArchidektService
{
    private readonly IArchidektApiClient _archidektApiClient = archidektApiClient;
    private readonly ILogger<ArchidektService> _logger = logger;

    public async Task<DeckDetailsDTO?> RetrieveDeckFromWeb(string deckUrl)
    {
        TryExtractDeckIdFromUrl(deckUrl, out int deckId);
        
        DeckDetailsDTO? deck = null;

        var deckDto = await _archidektApiClient.GetDeck(deckId);
        if (deckDto is null)
        {
            _logger.LogError("Deck not loaded from internet");
            return null;
        }
        if (deckDto.Cards is null || deckDto.Cards.Count == 0)
        {
            _logger.LogError("Deck is empty");
            return null;
        }

        deck = new DeckDetailsDTO { Name = deckDto.Name!, Cards = ParseCardsToDeck(deckDto.Cards!) };

        return deck;
    }

    public bool TryExtractDeckIdFromUrl(string url, out int deckId)
    {
        deckId = 0;
        string pattern = @"^https:\/\/archidekt\.com\/(?:api\/decks\/(\d+)\/|decks\/(\d+)\/)";
        Regex regex = new(pattern);

        Match match = regex.Match(url);
        if (match.Success)
        {
            for (int i = 1; i < match.Groups.Count; i++)
            {
                if (int.TryParse(match.Groups[i].Value, out deckId))
                {
                    return true;
                }
            }
        }

        return false;
    }


    private List<CardEntryDTO> ParseCardsToDeck(ICollection<DeckCardDTO> cardList)
    {
        List<CardEntryDTO> deckCards = []; 
        foreach (var card in cardList)
        {
            var cardName = card.Card?.OracleCard?.Name;
            if (cardName is null || card.Quantity <= 0)
            {
                continue;
            }

            deckCards.Add(new CardEntryDTO
            {
                Name = cardName,
                Quantity = card.Quantity,
                CollectorNumber = card.Card?.CollectorNumber,
                ExpansionCode = card.Card?.Edition?.EditionCode,
                Art = string.Equals(card.Card?.OracleCard?.Layout, "art_series", StringComparison.OrdinalIgnoreCase),
                Etched = string.Equals(card.Modifier, "Etched", StringComparison.OrdinalIgnoreCase),
                Foil = string.Equals(card.Modifier, "Foil", StringComparison.OrdinalIgnoreCase),
            });
        }

        return deckCards;
    }
}