using Library.IO;
using Library.Models.DTO;
using Library.Models.Events;
using Library.Services;

namespace Library;

/// <summary>
/// Represents a printer for generating Word documents from Archidekt deck data.
/// </summary>
public interface IArchidektPrinter
{
    /// <summary>
    /// Event that is raised to provide progress updates during the generation process.
    /// </summary>
    event EventHandler<UpdateProgressEventArgs>? ProgressUpdate;

    /// <summary>
    /// Generates a Word document from the specified deck data.
    /// </summary>
    /// <param name="deckId">The ID of the deck to generate the document for. If null, the deck will be loaded from the input file path.</param>
    /// <param name="inputFilePath">The path to the input file containing the deck data. If null, the deck will be loaded from the deck ID.</param>
    /// <param name="outputPath">The path to save the generated Word document. If null, the document will be saved in the default output directory.</param>
    /// <param name="outputFileName">The name of the generated Word document. If null, a default name will be used.</param>
    /// <param name="languageCode">The language code to be used for generating the Word document. If null, the default language code will be used.</param>
    /// <param name="saveImages">Specifies whether to save card images in the document. Default is false.</param>
    /// <returns>A task representing the asynchronous generation process.</returns>
    Task GenerateWord(int? deckId = null, string? inputFilePath = null, string? outputPath = null, string? outputFileName = null, string? languageCode = null, int tokenCopies = 0, bool printAllTokens = false, bool saveImages = false);

    /// <summary>
    /// Generates a Word document from the specified deck ID.
    /// </summary>
    /// <param name="deckId">The ID of the deck to generate the document for.</param>
    /// <param name="outputPath">The path to save the generated Word document. If null, the document will be saved in the default output directory.</param>
    /// <param name="outputFileName">The name of the generated Word document. If null, a default name will be used.</param>
    /// <param name="languageCode">The language code to be used for generating the Word document. If null, the default language code will be used.</param>
    /// <param name="saveImages">Specifies whether to save card images in the document. Default is false.</param>
    /// <returns>A task representing the asynchronous generation process.</returns>
    Task GenerateWordFromDeckOnline(int deckId, string? outputPath = null, string? outputFileName = null, string? languageCode = null, int tokenCopies = 0, bool printAllTokens = false, bool saveImages = false);

    /// <summary>
    /// Generates a Word document from the specified deck list file.
    /// </summary>
    /// <param name="deckListFilePath">The path to the file containing the deck list.</param>
    /// <param name="outputPath">The path to save the generated Word document. If null, the document will be saved in the default output directory.</param>
    /// <param name="outputFileName">The name of the generated Word document. If null, a default name will be used.</param>
    /// <param name="saveImages">Specifies whether to save card images in the document. Default is false.</param>
    /// <returns>A task representing the asynchronous generation process.</returns>
    Task GenerateWordFromDeckInFile(string deckListFilePath, string? outputPath = null, string? outputFileName = null, string? languageCode = null, int tokenCopies = 0, bool printAllTokens = false, bool saveImages = false);
}

public class ArchidektPrinter : IArchidektPrinter
{
    public event EventHandler<UpdateProgressEventArgs>? ProgressUpdate;

    private readonly IMagicCardService _magicCardService;
    private readonly IWordGeneratorService _wordGeneratorService;
    private readonly IFileManager _fileManager;
    private readonly ICardListFileParser _fileParser;

    public ArchidektPrinter(
        IMagicCardService magicCardService,
        IWordGeneratorService wordGeneratorService,
        IFileManager fileManager,
        ICardListFileParser fileParser
        )
    {
        _magicCardService = magicCardService;
        _wordGeneratorService = wordGeneratorService;
        _fileManager = fileManager;
        _fileParser = fileParser;

        _magicCardService.GetDeckDetailsProgress += UpdateGetDeckDetails;
        _wordGeneratorService.GenerateWordProgress += UpdateGenerateWord;
    }

    public async Task GenerateWord(
        int? deckId = null, 
        string? inputFilePath = null, 
        string? outputPath = null, 
        string? outputFileName = null,
        string? languageCode = null,
        int tokenCopies = 0, 
        bool printAllTokens = false,
        bool saveImages = false)
    {
        if (deckId != null) await GenerateWordFromDeckOnline(deckId!.Value, outputPath, outputFileName, languageCode, tokenCopies, printAllTokens, saveImages);
        else if (inputFilePath != null) await GenerateWordFromDeckInFile(inputFilePath, outputPath, outputFileName, languageCode, tokenCopies, printAllTokens, saveImages);
        else throw new ArgumentException("DeckId has to be bigger than 0 or WordFilePath has to be corrected");
    }

    public async Task GenerateWordFromDeckOnline(
        int deckId, 
        string? outputPath = null, 
        string? outputFileName = null, 
        string? languageCode = null,
        int tokenCopies = 0, 
        bool printAllTokens = false,
        bool saveImages = false)
    {
        var deckDetails = await _magicCardService.GetDeckWithCardPrintDetails(deckId, languageCode, tokenCopies, printAllTokens);
        if (deckDetails is null)
        {
            RaiseError("Getting deck details returned error");
            return;
        }

        await WriteDeckToWord(deckDetails, outputPath, outputFileName ?? deckDetails.Name, saveImages);
    }

    public async Task GenerateWordFromDeckInFile(
        string deckListFilePath, 
        string? outputPath = null, 
        string? outputFileName = null, 
        string? languageCode = null,
        int tokenCopies = 0, 
        bool printAllTokens = false,
        bool saveImages = false)
    {
        var deck = _fileParser.GetDeckFromFile(deckListFilePath);
        await _magicCardService.UpdateCardImageLinks(deck.Cards, languageCode, tokenCopies, printAllTokens);

        outputFileName ??= _fileManager.GetFilename(deckListFilePath);
        await WriteDeckToWord(deck, outputPath, outputFileName, saveImages);
    }


    private async Task WriteDeckToWord(
        DeckDetailsDTO deck, 
        string? outputPath = null, 
        string? outputFileName = null,
        bool saveImages = false)
    {
        if (deck.Cards.Count == 0)
        {
            RaiseError("Empty deck list");
            return;
        }

        outputPath = _fileManager.CreateOutputFolder(outputPath);
        var wordFilePath = _fileManager.ReturnCorrectWordFilePath(outputPath, outputFileName);

        await _wordGeneratorService.GenerateWord(deck, outputPath, wordFilePath, saveImages);
    }


    private void RaiseError(string? errorMessage = null)
    {
        ProgressUpdate?.Invoke(this, new UpdateProgressEventArgs
        {
            ErrorMessage = errorMessage
        });
    }

    private void UpdateGetDeckDetails(object? sender, GetDeckDetailsProgressEventArgs e)
    {
        ProgressUpdate?.Invoke(this, new UpdateProgressEventArgs
        {
            Stage = CreateMagicDeckDocumentStageEnum.GetDeckDetails,
            Percent = e.Percent,
            ErrorMessage = e.ErrorMessage
        });
    }

    private void UpdateGenerateWord(object? sender, GenerateWordProgressEventArgs e)
    {
        ProgressUpdate?.Invoke(this, new UpdateProgressEventArgs
        {
            Stage = CreateMagicDeckDocumentStageEnum.SaveToDocument,
            Percent = e.Percent,
            ErrorMessage = e.ErrorMessage
        });
    }
}
