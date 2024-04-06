using Library.Clients;
using Library.Models.DTO;
using Library.Models.DTO.Archidekt;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Library.Services;

public class DeckService(ILogger<DeckService> logger, ArchidektApiClient archidektApiClient, ScryfallApiClient scryfallApiClient)
{
    private readonly ILogger<DeckService> _logger = logger;
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

        deck = new DeckDetailsDTO { Name = deckDto.Name, Cards = ParseCardsToDeck(deckDto.Cards!) };

        await UpdateCardImageLinks(deck.Cards);

        return deck;
    }

    // TODO Refactor this to separate service
    public async Task<byte[]?> GetImage(string url) => await _scryfallApiClient.GetImage(url);

    public async Task DownloadCards(Dictionary<string, CardEntryDTO> cards, string outputPath)
    {
        try
        {
            foreach (var card in cards)
            {
                var cardData = card.Value;
                var images = await _scryfallApiClient.GetCardImageUrlsFromScryfall(cardData.Name);
                if (images != null)
                {
                    foreach (var image in images)
                    {
                        await DownloadImage(image.Value, outputPath, cardData.Name, cardData.Quantity);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in downloading cards from the Scryfall");
        }
    }

    // TODO Refactor this to separate service
    public async Task DownloadImage(string imageUrl, string folderPath, string filename, int quantity)
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

    
    private Dictionary<string, CardEntryDTO> ParseCardsToDeck(ICollection<DeckCardDTO> cardList)
    {
        Dictionary<string, CardEntryDTO> deckCard = []; 
        foreach (var card in cardList)
        {
            var cardName = card.Card?.OracleCard?.Name;
            if (cardName is null || card.Quantity <= 0)
            {
                continue;
            }

            if (deckCard.TryGetValue(cardName, out CardEntryDTO? value))
            {
                value.Quantity++;
            }
            else
            {
                deckCard.Add(cardName, new CardEntryDTO
                {
                    Name = cardName,
                    Quantity = card.Quantity
                });
            }
        }

        return deckCard;
    }

    private async Task UpdateCardImageLinks(Dictionary<string, CardEntryDTO> cards)
    {
        try
        {
            foreach (var card in cards)
            {
                var cardData = card.Value;
                var images = await _scryfallApiClient.GetCardImageUrlsFromScryfall(cardData.Name);
                if (images != null)
                {
                    cardData.ImageUrls = images;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in downloading cards from the Scryfall");
        }
    }
}