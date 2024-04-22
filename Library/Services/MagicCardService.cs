using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

using Library.Clients;
using Library.Models.DTO;
using Library.Models.DTO.Archidekt;
using Library.Models.Events;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Library.Services;

public class MagicCardService(ILogger<MagicCardService> logger, ArchidektApiClient archidektApiClient, ScryfallApiClient scryfallApiClient)
{
    public event EventHandler<GetDeckDetailsProgressEventArgs>? GetDeckDetailsProgress;

    private readonly ILogger<MagicCardService> _logger = logger;
    private readonly ArchidektApiClient _archidektApiClient = archidektApiClient;
    private readonly ScryfallApiClient _scryfallApiClient = scryfallApiClient;


    public async Task<DeckDetailsDTO?> GetDeckWithCardPrintDetails(int deckId)
    {
        DeckDetailsDTO? deck = null;
        
        var deckDto = await _archidektApiClient.GetDeck(deckId);
        if (deckDto is null)
        {
            _logger.LogError("Deck not loaded from internet");
            return deck;
        }
        if (deckDto.Cards is null || deckDto.Cards.Count == 0)
        {
            _logger.LogError("Deck is empty");
            return deck;
        }

        deck = new DeckDetailsDTO { Name = deckDto.Name!, Cards = ParseCardsToDeck(deckDto.Cards!) };

        await UpdateCardImageLinks(deck.Cards);

        return deck;
    }

    public async Task DownloadCardSideImage(string imageUrl, string folderPath, string filename, int quantity)
    {
        try
        {
            var imageBytes = await _scryfallApiClient.GetImage(imageUrl);
            if (imageBytes is null)
            {
                _logger.LogWarning("Image not received from internet");
                return;
            }

            var filePath = Path.Combine(folderPath, $"{quantity}_{filename.Replace(" // ", "-")}.jpg");
            await File.WriteAllBytesAsync(filePath, imageBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in downloading image from the Scryfall");
        }
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
                Etched = string.Equals(card.Modifier, "Etched", StringComparison.OrdinalIgnoreCase),
                Foil = string.Equals(card.Modifier, "Foil", StringComparison.OrdinalIgnoreCase),
            });
        }

        return deckCards;
    }

    private async Task UpdateCardImageLinks(List<CardEntryDTO> cards)
    {
        try
        {
            int count = cards.Count();
            int step = UpdateStep(0, count);
            foreach (var card in cards)
            {
                var images = await GetCardImageUrls(card);
                if (images != null)
                {
                    card.CardSides = images;
                }
                step = UpdateStep(step, count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in downloading cards from the Scryfall");
        }
    }

    private async Task<HashSet<CardSideDTO>?> GetCardImageUrls(CardEntryDTO card)
    {
        var cardSearch = await _scryfallApiClient.FindCard(card);

        HashSet<CardSideDTO> cardSides = [];

        // Look for searched card in the search result
        var searchedCard = cardSearch?.Data?.FirstOrDefault(c => c != null && c.Name != null && c.Name.Equals(card.Name, StringComparison.OrdinalIgnoreCase));
        if (searchedCard is null)
        {
            _logger.LogWarning("Card {card.Name} was not found in the Scryfall database", card.Name);
            return null;
        }
        
        // Handle two dual side cards as well
        if (searchedCard.Card_faces is not null)
        {
            foreach (var cardFace in searchedCard.Card_faces)
            {
                if (cardFace.Image_uris is null)
                {
                    continue;
                }

                cardSides.Add(new CardSideDTO { Name = cardFace.Name ?? string.Empty, ImageUrl = cardFace.Image_uris?.Large ?? string.Empty });
            }
        }

        // Single page card
        if (cardSides.Count == 0 || cardSides.Any(i => string.IsNullOrEmpty(i.Name) || string.IsNullOrEmpty(i.ImageUrl)))
        {
            cardSides.Clear();
            if (searchedCard.Image_uris?.Large is null)
            {
                _logger.LogError("Card {Name} does not have any url to its picture", card.Name);
                return null;
            }
            cardSides.Add(new CardSideDTO { Name = card.Name, ImageUrl = searchedCard.Image_uris.Large });
        }

        return cardSides;
    }


    private int UpdateStep(int step, int count)
    {
        var percent = (double)step / count * 100;
        GetDeckDetailsProgress?.Invoke(this, new GetDeckDetailsProgressEventArgs
        {
            Percent = percent
        });
        return ++step;
    }
}