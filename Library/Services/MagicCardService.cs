using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

using Library.Clients;
using Library.Models.DTO;
using Library.Models.DTO.Archidekt;
using Library.Models.Events;

namespace Library.Services;

/// <summary>
/// Represents a service for interacting with Magic cards.
/// </summary>
public interface IMagicCardService
{
    /// <summary>
    /// Event that is raised to report the progress of getting deck details.
    /// </summary>
    event EventHandler<GetDeckDetailsProgressEventArgs>? GetDeckDetailsProgress;

    /// <summary>
    /// Downloads the image of a card side.
    /// </summary>
    /// <param name="imageUrl">The URL of the image to download.</param>
    /// <param name="folderPath">The path to the folder where the image will be saved.</param>
    /// <param name="filename">The name of the image file.</param>
    /// <param name="quantity">The quantity of the card side.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DownloadCardSideImage(string imageUrl, string folderPath, string filename, int quantity);

    /// <summary>
    /// Gets the deck details along with card print details.
    /// </summary>
    /// <param name="deckId">The ID of the deck.</param>
    /// <param name="languageCode">The language code for localization (optional).</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the deck details DTO, or null if not found.</returns>
    Task<DeckDetailsDTO?> GetDeckWithCardPrintDetails(int deckId, string? languageCode = null);

    /// <summary>
    /// Tries to extract the deck ID from a URL.
    /// </summary>
    /// <param name="url">The URL to extract the deck ID from.</param>
    /// <param name="deckId">When this method returns, contains the extracted deck ID if successful, or 0 if unsuccessful.</param>
    /// <returns>true if the deck ID was successfully extracted from the URL; otherwise, false.</returns>
    bool TryExtractDeckIdFromUrl(string url, out int deckId);

    /// <summary>
    /// Updates the card image links.
    /// </summary>
    /// <param name="cards">The list of card entries to update.</param>
    /// <param name="languageCode">The language code for localization (optional).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateCardImageLinks(List<CardEntryDTO> cards, string? languageCode = null);
}

public class MagicCardService(ILogger<MagicCardService> logger, ArchidektApiClient archidektApiClient, ScryfallApiClient scryfallApiClient) : IMagicCardService
{
    public event EventHandler<GetDeckDetailsProgressEventArgs>? GetDeckDetailsProgress;

    private readonly ILogger<MagicCardService> _logger = logger;
    private readonly ArchidektApiClient _archidektApiClient = archidektApiClient;
    private readonly ScryfallApiClient _scryfallApiClient = scryfallApiClient;


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

    public async Task<DeckDetailsDTO?> GetDeckWithCardPrintDetails(int deckId, string? languageCode = null)
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

        await UpdateCardImageLinks(deck.Cards, languageCode);

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

    public async Task UpdateCardImageLinks(List<CardEntryDTO> cards, string? languageCode = null)
    {
        try
        {
            int count = cards.Count;
            int step = UpdateStep(0, count);
            foreach (var card in cards)
            {
                var images = await GetCardImageUrls(card, languageCode);
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

    private async Task<HashSet<CardSideDTO>?> GetCardImageUrls(CardEntryDTO card, string? languageCode = null)
    {
        // Check if we have enough details about specific card for a Find instead of Search
        var cardSearch = card.ExpansionCode != null && card.CollectorNumber != null
            ? new() { Data = [await _scryfallApiClient.FindCard(card.Name, card.ExpansionCode, card.CollectorNumber, languageCode)] }
            : await _scryfallApiClient.SearchCard(card.Name, card.ExpansionCode is not null || card.Etched || card.Art, languageCode != null);

        HashSet<CardSideDTO> cardSides = [];

        // Look for searched card in the search result
        var searchedCard = cardSearch?.Data?.FirstOrDefault(c => 
            c != null && c.Name != null && c.Name.Equals(card.Name, StringComparison.OrdinalIgnoreCase) // Find by name
            && ((!card.Etched) || (card.Etched && c.TcgplayerEtchedId is not null)) // Find etched frame if required
            && ((card.ExpansionCode == null) || string.Equals(card.ExpansionCode, c.Set)) // Find by expansion if required
            && ((languageCode == null) || c.Lang!.Equals(languageCode, StringComparison.OrdinalIgnoreCase)) // Find by expansion if required
            );
        if (searchedCard is null)
        {
            _logger.LogWarning("Card {Name} was not found in the Scryfall database", card.Name);
            return null;
        }
        
        // Handle two dual side cards as well
        if (searchedCard.CardFaces is not null)
        {
            foreach (var cardFace in searchedCard.CardFaces)
            {
                if (cardFace.ImageUris is null)
                {
                    continue;
                }

                cardSides.Add(new CardSideDTO { Name = cardFace.Name ?? string.Empty, ImageUrl = cardFace.ImageUris?.Large ?? string.Empty });
            }
        }

        var nameSplit = card.Name.Split(" // ");
        // Remove back side of art as it is simple placeholder
        if ((card.Art || 
            (nameSplit.Count() > 1 && nameSplit[0] == nameSplit[1])) 
            && cardSides.Count > 0)
        {
            cardSides = [cardSides.First()];
        }

        // Single page card
        if (cardSides.Count == 0 || cardSides.Any(i => string.IsNullOrEmpty(i.Name) || string.IsNullOrEmpty(i.ImageUrl)))
        {
            cardSides.Clear();
            if (searchedCard.ImageUris?.Large is null)
            {
                _logger.LogError("Card {Name} does not have any url to its picture", card.Name);
                return null;
            }
            cardSides.Add(new CardSideDTO { Name = card.Name, ImageUrl = searchedCard.ImageUris.Large });
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