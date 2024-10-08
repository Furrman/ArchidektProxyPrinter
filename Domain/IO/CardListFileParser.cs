using Microsoft.Extensions.Logging;

using Domain.Models.DTO;

namespace Domain.IO;

/// <summary>
/// Represents a parser for card list files.
/// </summary>
public interface ICardListFileParser
{
    /// <summary>
    /// Parses a card list file and returns the deck details.
    /// </summary>
    /// <param name="filePath">The path to the card list file.</param>
    /// <returns>The deck details.</returns>
    DeckDetailsDTO GetDeckFromFile(string filePath);
}

public class CardListFileParser(ILogger<CardListFileParser> logger, IFileManager fileManager)
    : ICardListFileParser
{
    private readonly ILogger<CardListFileParser> _logger = logger;
    private readonly IFileManager _fileManager = fileManager;

    public DeckDetailsDTO GetDeckFromFile(string filePath)
    {
        DeckDetailsDTO deck = new();

        try
        {
            deck.Name = _fileManager.GetFilename(filePath);
            var lines = _fileManager.GetLinesFromTextFile(filePath);
            if (lines == null)
            {
                return deck;
            }

            foreach (var line in lines)
            {
                var cardData = ParseLine(line);
                if (cardData == null)
                {
                    continue;
                }
                deck.Cards.Add(cardData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in parsing card list from file");
        }

        return deck;
    }


    private static CardEntryDTO? ParseLine(string line)
    {
        var cardEntry = new CardEntryDTO();

        // Split line into parts
        string[] parts = line.Split(' ');
        int nameStartIndex = line.IndexOf(' ') + 1;

        // Parse quantity
        string quantityPart = parts[0];
        if (quantityPart.Length > 1 && quantityPart.Contains('x'))
        {
            quantityPart = quantityPart.Remove(quantityPart.Length - 1); // Remove last character from quantity
        }
        if (!int.TryParse(quantityPart, out int quantity))
        {
            quantity = 1;
            nameStartIndex = 0;
        }
        cardEntry.Quantity = quantity;

        // Find index of optional parts
        int expansionStartIndex = line.IndexOf('(');
        int expansionEndIndex = line.IndexOf(')');
        int foilIndex = line.IndexOf("*F*");
        int etchedIndex = line.IndexOf("*E*");

        // Parse card name
        int[] optionalElementIndexes = { expansionStartIndex, foilIndex, etchedIndex };
        int nameEndIndex = optionalElementIndexes.Where(n => n != -1).DefaultIfEmpty(line.Length).Min();
        cardEntry.Name = line[nameStartIndex..nameEndIndex].TrimEnd();

        // Parse optional parts
        if (expansionStartIndex != -1)
        {
            cardEntry.ExpansionCode = line[(expansionStartIndex + 1)..expansionEndIndex].TrimEnd();
        }
        if (foilIndex != -1)
        {
            cardEntry.Foil = true;
        }
        if (etchedIndex != -1)
        {
            cardEntry.Etched = true;
        }

        return cardEntry;
    }
}
