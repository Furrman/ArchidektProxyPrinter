using Library.Clients;
using Library.Models.DTO;
using System.Text.RegularExpressions;

namespace Library.Services;

public class DeckService(ArchidektApiClient archidektApiClient, ScryfallApiClient scryfallApiClient)
{
    private readonly ArchidektApiClient _archidektApiClient = archidektApiClient;
    private readonly ScryfallApiClient _scryfallApiClient = scryfallApiClient;


    public async Task<DeckDetailsDTO?> GetDeckDetails(int deckId)
    {
        DeckDetailsDTO? deck = null;
        
        var deckDto = await _archidektApiClient.GetDeck(deckId);
        if (deckDto is null || deckDto?.Cards is null || deckDto.Cards?.Count == 0)
        {
            return deck;
        }

        deck = new DeckDetailsDTO { Name = deckDto.Name };
        foreach (var card in deckDto.Cards!)
        {
            var cardName = card.Card?.OracleCard?.Name;
            if (cardName is null || card.Quantity <= 0)
            {
                continue;
            }

            if (deck.Cards.TryGetValue(cardName, out CardEntryDTO? value))
            {
                value.Quantity++;
            }
            else
            {
                deck.Cards.Add(cardName, new CardEntryDTO
                {
                    Name = cardName,
                    Quantity = card.Quantity
                });
            }
        }

        await _scryfallApiClient.UpdateCardImageLinks(deck.Cards);

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
}