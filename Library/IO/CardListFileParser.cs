using Library.Models.DTO;
using Microsoft.Extensions.Logging;

namespace Library.IO;

public class CardListFileParser(ILogger<CardListFileParser> logger, FileManager fileManager)
{
    private readonly ILogger<CardListFileParser> _logger = logger;
    private readonly FileManager _fileManager = fileManager;

    public DeckDetailsDTO GetDeckFromFile(string filePath)
    {
        DeckDetailsDTO deck = new();

        try
        {
            deck.Name = _fileManager.GetFilename(filePath);
            using var reader = new StreamReader(filePath);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                // TODO Add support for more complex file exporter from Archidekt
                var cardData = ParseLine(line);
                if (cardData == null)
                {
                    continue;
                }
                deck.Cards.Add(new CardEntryDTO
                {
                    Name = cardData.Value.Item1,
                    Quantity = cardData.Value.Item2
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in parsing card list from file");
        }

        return deck;
    }


    private (string, int)? ParseLine(string line)
    {
        int quantity;
        string cardName;
        string[] parts = line.Split(' ');

        if (parts.Length >= 2)
        {
            if (int.TryParse(parts[0], out quantity))
            {
                cardName = string.Join(" ", parts, 1, parts.Length - 1);
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }

        return (cardName, quantity);
    }
}
