using Microsoft.Extensions.Logging;

using DocumentFormat.OpenXml.Wordprocessing;
using OfficeIMO.Word;

using Domain.Constants;
using Domain.Clients;
using Domain.Models.DTO;
using Domain.Models.Events;
using Domain.IO;

namespace Domain.Services;

/// <summary>
/// Represents a service for generating Word documents based on deck details.
/// </summary>
public interface IWordGeneratorService
{
    /// <summary>
    /// Event that is raised to report the progress of the word generation.
    /// </summary>
    event EventHandler<GenerateWordProgressEventArgs>? GenerateWordProgress;

    /// <summary>
    /// Generates a Word document based on the provided deck details.
    /// </summary>
    /// <param name="deck">The deck details.</param>
    /// <param name="wordFileName">The file name of the Word document without extension.</param>
    /// <param name="outputFolder">The output folder where the Word document and optional images will be saved.</param>
    /// <param name="saveImages">A flag indicating whether to save images in the Word document.</param>
    /// <returns>A task representing the asynchronous generation of the Word document.</returns>
    Task GenerateWord(DeckDetailsDTO deck, string? wordFileName = null, string? outputFolder = null, bool saveImages = false);
}

public class WordGeneratorService(ILogger<WordGeneratorService> logger, IScryfallClient scryfallClient, IWordDocumentWrapper wordDocumentWrapper, IFileManager fileManager)
    : IWordGeneratorService
{
    public event EventHandler<GenerateWordProgressEventArgs>? GenerateWordProgress;

    private readonly ILogger<WordGeneratorService> _logger = logger;
    private readonly IScryfallClient _scryfallClient = scryfallClient;
    private readonly IWordDocumentWrapper _wordDocumentWrapper = wordDocumentWrapper;
    private readonly IFileManager _fileManager = fileManager;


    public async Task GenerateWord(DeckDetailsDTO deck, string? wordFileName = null, string? outputFolderDir = null, bool saveImages = false)
    {
        try
        {
            int count = deck.Cards.SelectMany(c => c.CardSides).Count();
            if (count == 0)
            {
                RaiseError("No cards found in the deck");
                return;
            }
            var outputFolderPath = _fileManager.CreateOutputFolder(outputFolderDir);
            if (outputFolderPath is null)
            {
                RaiseError("Error in creating output folder");
                return;
            }
            var wordFilePath = _fileManager.ReturnCorrectWordFilePath(outputFolderPath, wordFileName ?? deck.Name);

            using WordDocument document = _wordDocumentWrapper.Create(wordFilePath);
            _wordDocumentWrapper.SetMargins(WordMargin.Narrow);
            _wordDocumentWrapper.SetOrientation(PageOrientationValues.Landscape);
            _wordDocumentWrapper.SetPageSize(WordPageSize.A4);
            var paragraph = _wordDocumentWrapper.AddParagraph();

            int step = UpdateStep(0, count);
            foreach (var card in deck.Cards)
            {
                foreach (var cardSide in card.CardSides)
                {
                    var imageContent = await _scryfallClient.DownloadImage(cardSide.ImageUrl);
                    if (imageContent == null)
                    {
                        step = UpdateStep(step, count);
                        continue;
                    }
                    if (saveImages)
                    {
                        await _fileManager.CreateImageFile(imageContent, outputFolderPath, cardSide.ImageUrl);
                    }

                    AddImageToWord(paragraph, cardSide.Name, imageContent, card.Quantity);

                    step = UpdateStep(step, count);
                }
            }

            _wordDocumentWrapper.Save();
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
            for (int i = 0; i < quantity; i++)
            {
                _wordDocumentWrapper.AddImage(paragraph, imageContent, $"{imageName}{i}", width: CardDetails.CARD_WIDTH_PIXELS, height: CardDetails.CARD_HEIGHT_PIXELS);
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
            Percent = 100,
            ErrorMessage = errorMessage
        });
    }

    private int UpdateStep(int step, int count)
    {
        var percent = (double)step / count * 100;
        GenerateWordProgress?.Invoke(this, new GenerateWordProgressEventArgs
        {
            Percent = percent
        });
        return ++step;
    }
}
