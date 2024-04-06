using Library.Clients;
using System.Text.RegularExpressions;

namespace Library.Services;

public class DeckService
{
    private readonly ArchidektApiClient _archidektApiClient;
    private readonly ScryfallApiClient _scryfallApiClient;

    public DeckService(ArchidektApiClient archidektApiClient, ScryfallApiClient scryfallApiClient)
    {
        _archidektApiClient = archidektApiClient;
        _scryfallApiClient = scryfallApiClient;
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