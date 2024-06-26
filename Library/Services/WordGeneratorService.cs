﻿using Microsoft.Extensions.Logging;

using DocumentFormat.OpenXml.Wordprocessing;
using OfficeIMO.Word;

using Library.Clients;
using Library.Models.DTO;
using Library.Models.Events;
using Library.IO;

namespace Library.Services;

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
    /// <param name="outputFolder">The output folder where the Word document will be saved.</param>
    /// <param name="wordFilePath">The file path of the Word document.</param>
    /// <param name="saveImages">A flag indicating whether to save images in the Word document.</param>
    /// <returns>A task representing the asynchronous generation of the Word document.</returns>
    Task GenerateWord(DeckDetailsDTO deck, string outputFolder, string wordFilePath, bool saveImages);
}

public class WordGeneratorService(ILogger<WordGeneratorService> logger, IScryfallApiClient scryfallClient, IFileManager fileManager)
    : IWordGeneratorService
{
    public event EventHandler<GenerateWordProgressEventArgs>? GenerateWordProgress;

    private readonly ILogger<WordGeneratorService> _logger = logger;
    private readonly IScryfallApiClient _scryfallClient = scryfallClient;
    private readonly IFileManager _fileManager = fileManager;


    public async Task GenerateWord(DeckDetailsDTO deck, string outputFolder, string wordFilePath, bool saveImages)
    {
        try
        {
            using WordDocument document = WordDocument.Create(wordFilePath);

            document.Margins.Type = WordMargin.Narrow;
            document.PageSettings.Orientation = PageOrientationValues.Landscape;
            document.PageSettings.PageSize = WordPageSize.A4;

            var paragraph = document.AddParagraph();

            int count = deck.Cards.SelectMany(c => c.CardSides).Count();
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
                        await _fileManager.CreateImageFile(imageContent, outputFolder, cardSide.ImageUrl);
                    }

                    AddImageToWord(paragraph, cardSide.Name, imageContent, card.Quantity);

                    step = UpdateStep(step, count);
                }
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
            for (int i = 0; i < quantity; i++)
            {
                using MemoryStream stream = new(imageContent);
                paragraph.AddImage(stream, $"{imageName}{i}", width: CardDetails.CARD_WIDTH_PIXELS, height: CardDetails.CARD_HEIGHT_PIXELS);
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
