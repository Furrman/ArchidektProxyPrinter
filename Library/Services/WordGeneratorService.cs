using Microsoft.Extensions.Logging;

using DocumentFormat.OpenXml.Wordprocessing;
using OfficeIMO.Word;

using Library.Clients;
using Library.Models.DTO;
using Library.Models.Events;
using Library.IO;

namespace Library.Services;

public class WordGeneratorService(ILogger<WordGeneratorService> logger, ScryfallApiClient scryfallClient, FileManager fileManager)
{
    public event EventHandler<GenerateWordProgressEventArgs>? GenerateWordProgress;

    private readonly ILogger<WordGeneratorService> _logger = logger;
    private readonly ScryfallApiClient _scryfallClient = scryfallClient;
    private readonly FileManager _fileManager = fileManager;


    public async Task GenerateWord(DeckDetailsDTO deck, string outputFolder, string wordFilePath, bool saveImages)
    {
        try
        {
            using WordDocument document = WordDocument.Create(wordFilePath);

            document.Margins.Type = WordMargin.Narrow;
            document.PageSettings.Orientation = PageOrientationValues.Landscape;
            document.PageSettings.PageSize = WordPageSize.A4;

            var paragraph = document.AddParagraph();

            double step = 0;
            double count = deck.Cards.SelectMany(c => c.Value.ImageUrls).Count();

            var entries = deck.Cards
                .SelectMany(card => card.Value.ImageUrls)
                .Select(entry => new { CardSide = entry, CardQuantity = deck.Cards[entry.Key].Quantity });
            foreach (var entry in entries)
            {
                var imageContent = await _scryfallClient.GetImage(entry.CardSide.Value);
                if (imageContent == null)
                {
                    step = UpdateStep(step, count);
                    continue;
                }
                if (saveImages)
                {
                    await _fileManager.CreateImageFile(imageContent, outputFolder, entry.CardSide.Key);
                }

                AddImageToWord(paragraph, entry.CardSide.Key, imageContent, entry.CardQuantity);

                step = UpdateStep(step, count);
            }

            document.Save();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in writting images to Word file");
            RaiseError("Error in generating word");
        }
    }


    private void AddImageToWord(WordParagraph paragraph, string imageName, byte[] imageContent, int quantity)
    {
        try
        {
            using MemoryStream stream = new(imageContent);
            for (int i = 0; i < quantity; i++)
            {
                paragraph.AddImage(stream, imageName, width: Constants.CARD_WIDTH_PIXELS, height: Constants.CARD_HEIGHT_PIXELS);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in adding image to word file");
        }
    }


    private void RaiseError(string? errorMessage = null)
    {
        GenerateWordProgress?.Invoke(this, new GenerateWordProgressEventArgs
        {
            ErrorMessage = errorMessage
        });
    }

    private double UpdateStep(double step, double count)
    {
        step++;
        var percent = step / count * 100;
        GenerateWordProgress?.Invoke(this, new GenerateWordProgressEventArgs
        {
            Percent = percent
        });
        return step;
    }
}
