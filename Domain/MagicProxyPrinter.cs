using Domain.Factories;
using Domain.IO;
using Domain.Models.Events;
using Domain.Services;

namespace Domain;

/// <summary>
/// Facade that provides a simplified interface for generating Magic: The Gathering proxy cards.
/// </summary>
public interface IMagicProxyPrinter
{
    /// <summary>
    /// Event that is raised to provide progress updates during the generation process.
    /// </summary>
    event EventHandler<UpdateProgressEventArgs>? ProgressUpdate;

    /// <summary>
    /// Generates a Word document from the specified deck data.
    /// </summary>
    /// <param name="deckUrl">The url the deck available online to generate the document for. If null, the deck will be loaded from the input file path.</param>
    /// <param name="inputFilePath">The path to the input file containing the deck data. If null, the deck will be loaded from the deck ID.</param>
    /// <param name="outputDirPath">The path to folder for output files. If null, the document will be saved in the default output directory.</param>
    /// <param name="outputFileName">The name of the generated Word document. If null, a default name will be used.</param>
    /// <param name="languageCode">The language code to be used for generating the Word document. If null, the default language code will be used.</param>
    /// <param name="saveImages">Specifies whether to save card images in the document. Default is false.</param>
    /// <returns>A task representing the asynchronous generation process.</returns>
    Task GenerateWord(string? deckUrl = null, string? inputFilePath = null, string? outputDirPath = null, string? outputFileName = null, string? languageCode = null, int tokenCopies = 0, bool printAllTokens = false, bool saveImages = false);

    /// <summary>
    /// Generates a Word document from the specified deck ID stored in the internet.
    /// </summary>
    /// <param name="deckUrl">The url the deck available online to generate the document for. If null, the deck will be loaded from the input file path.</param>
    /// <param name="outputDirPath">The path to folder for output files. If null, the document will be saved in the default output directory.</param>
    /// <param name="outputFileName">The name of the generated Word document. If null, a default name will be used.</param>
    /// <param name="languageCode">The language code to be used for generating the Word document. If null, the default language code will be used.</param>
    /// <param name="saveImages">Specifies whether to save card images in the document. Default is false.</param>
    /// <returns>A task representing the asynchronous generation process.</returns>
    Task GenerateWordFromDeckOnline(string deckUrl, string? outputDirPath = null, string? outputFileName = null, string? languageCode = null, int tokenCopies = 0, bool printAllTokens = false, bool saveImages = false);

    /// <summary>
    /// Generates a Word document from the specified deck list file.
    /// </summary>
    /// <param name="deckListFilePath">The path to the file containing the deck list.</param>
    /// <param name="outputDirPath">The path to folder for output files. If null, the document will be saved in the default output directory.</param>
    /// <param name="outputFileName">The name of the generated Word document. If null, a default name will be used.</param>
    /// <param name="saveImages">Specifies whether to save card images in the document. Default is false.</param>
    /// <returns>A task representing the asynchronous generation process.</returns>
    Task GenerateWordFromDeckInFile(string deckListFilePath, string? outputDirPath = null, string? outputFileName = null, string? languageCode = null, int tokenCopies = 0, bool printAllTokens = false, bool saveImages = false);
}

public class MagicProxyPrinter : IMagicProxyPrinter
{
    public event EventHandler<UpdateProgressEventArgs>? ProgressUpdate;

    private readonly IDeckRetrieverFactory _deckRetrieverFactory;
    private readonly IScryfallService _scryfallService;
    private readonly ICardListFileParser _fileParser;
    private readonly IFileManager _fileManager;
    private readonly IWordGeneratorService _wordGeneratorService;

    public MagicProxyPrinter(
        IDeckRetrieverFactory deckRetrieverFactory,
        IScryfallService scryfallService,
        ICardListFileParser fileParser,
        IFileManager fileManager,
        IWordGeneratorService wordGeneratorService)
    {
        _deckRetrieverFactory = deckRetrieverFactory;
        _scryfallService = scryfallService;
        _fileParser = fileParser;
        _fileManager = fileManager;
        _wordGeneratorService = wordGeneratorService;

        _scryfallService.GetDeckDetailsProgress += UpdateGetDeckDetails;
        _wordGeneratorService.GenerateWordProgress += UpdateGenerateWord;
    }

    public async Task GenerateWord(
        string? deckUrl = null, 
        string? inputFilePath = null, 
        string? outputDirPath = null, 
        string? outputFileName = null,
        string? languageCode = null,
        int tokenCopies = 0, 
        bool printAllTokens = false,
        bool saveImages = false)
    {
        if (deckUrl != null) await GenerateWordFromDeckOnline(deckUrl, outputDirPath, outputFileName, languageCode, tokenCopies, printAllTokens, saveImages);
        else if (inputFilePath != null) await GenerateWordFromDeckInFile(inputFilePath, outputDirPath, outputFileName, languageCode, tokenCopies, printAllTokens, saveImages);
        else throw new ArgumentException("Wrong input parameters to download deck.");
    }

    public async Task GenerateWordFromDeckOnline(
        string deckUrl, 
        string? outputDirPath = null, 
        string? outputFileName = null, 
        string? languageCode = null,
        int tokenCopies = 0, 
        bool printAllTokens = false,
        bool saveImages = false)
    {
        var deckRetriever = _deckRetrieverFactory.GetDeckRetriever(deckUrl);
        if (deckRetriever is null)
        {
            RaiseError("Not able to find deck online");
            return;
        }
        var deck = await deckRetriever.RetrieveDeckFromWeb(deckUrl);
        if (deck is null)
        {
            RaiseError("Getting deck details returned error");
            return;
        }

        await _scryfallService.UpdateCardImageLinks(deck.Cards, languageCode, tokenCopies, printAllTokens);

        await _wordGeneratorService.GenerateWord(deck, outputFileName, outputDirPath, saveImages);
    }

    public async Task GenerateWordFromDeckInFile(
        string deckListFilePath, 
        string? outputDirPath = null, 
        string? outputFileName = null, 
        string? languageCode = null,
        int tokenCopies = 0, 
        bool printAllTokens = false,
        bool saveImages = false)
    {
        if (!_fileManager.FileExists(deckListFilePath))
        {
            RaiseError("You have to specify correct PATH to your card list exported from Archidekt.");
            return;
        }

        var deck = _fileParser.GetDeckFromFile(deckListFilePath);
        if (deck is null)
        {
            RaiseError("Error in parsing deck list file");
            return;
        }

        await _scryfallService.UpdateCardImageLinks(deck.Cards, languageCode, tokenCopies, printAllTokens);

        await _wordGeneratorService.GenerateWord(deck, outputFileName, outputDirPath, saveImages);
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
