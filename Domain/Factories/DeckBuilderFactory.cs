using Domain.Models.DTO;
using Domain.Services;

namespace Domain.Factories;

/// <summary>
/// Interface to retrieve a deck from web.
/// </summary>
public interface IDeckRetriever
{
    /// <summary>
    /// Retrieve a deck from web.
    /// </summary>
    /// <param name="deckUrl">Url to the deck available online.</param>
    /// <returns>Deck with all details or null if error occurs.</returns>
    Task<DeckDetailsDTO?> RetrieveDeckFromWeb(string deckUrl);
}

/// <summary>
/// Factory for retriving correct service for obtaining a deck from web.
/// </summary>
public interface IDeckRetrieverFactory
{
    /// <summary>
    /// Get the correct service for obtaining a deck from web.
    /// </summary>
    /// <param name="deckUrl">Url to the deck available online.</param>
    /// <returns>Object able to retrieve deck from web or null if no website is matching url.</returns>
    IDeckRetriever? GetDeckRetriever(string deckUrl);
}

public class DeckRetrieverFactory(IArchidektService archidektService) : IDeckRetrieverFactory
{
    private readonly IArchidektService _archidektService = archidektService;

    public IDeckRetriever? GetDeckRetriever(string deckUrl)
    {
        if (_archidektService.TryExtractDeckIdFromUrl(deckUrl, out int deckId))
        {
            return _archidektService;
        }

        return null;
    }
}