using Library.IO;
using Library.Models.DTO;
using Library.Models.Events;
using Library.Services;

namespace Library;

public class ArchidektPrinter
{
    public event EventHandler<UpdateProgressEventArgs>? ProgressUpdate;

    private readonly MagicCardService _magicCardService;
    private readonly WordGeneratorService _wordGeneratorService;
    private readonly FileManager _fileManager;
    private readonly CardListFileParser _fileParser;

    public ArchidektPrinter(
        MagicCardService magicCardService,
        WordGeneratorService wordGeneratorService,
        FileManager fileManager,
        CardListFileParser fileParser
        )
    {
        _magicCardService = magicCardService;
        _wordGeneratorService = wordGeneratorService;
        _fileManager = fileManager;
        _fileParser = fileParser;

        _wordGeneratorService.GenerateWordProgress += UpdateGenerateWordStep;
    }

    public async Task GenerateWord(int? deckId = null, string? inputFilePath = null, string? outputPath = null, string? outputFileName = null, bool saveImages = false)
    {
        if (deckId != 0) await GenerateWord(deckId!.Value, outputPath, outputFileName, saveImages);
        else if (inputFilePath != null) await GenerateWord(inputFilePath, outputPath, outputFileName, saveImages);
        else throw new ArgumentException("DeckId has to be bigger than 0 or WordFilePath has to be corrected");
    }

    public async Task GenerateWord(int deckId, string? outputPath = null, string? outputFileName = null, bool saveImages = false)
    {
        var deckDetails = await _magicCardService.GetDeckWithCardPrintDetails(deckId);
        if (deckDetails is null)
        {
            RaiseError("Getting deck details returned error");
            return;
        }

        await GenerateWord(deckDetails, outputPath, outputFileName ?? deckDetails.Name, saveImages);
    }

    public async Task GenerateWord(string deckListFilePath, string? outputPath = null, string? outputFileName = null, bool saveImages = false)
    {
        var cardList = _fileParser.GetDeckFromFile(deckListFilePath);
        await GenerateWord(cardList, outputPath, outputFileName, saveImages);
    }


    private async Task GenerateWord(DeckDetailsDTO deck, string? outputPath = null, string? outputFileName = null, bool saveImages = false)
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

    private void UpdateGenerateWordStep(object? sender, GenerateWordProgressEventArgs e)
    {
        ProgressUpdate?.Invoke(this, new UpdateProgressEventArgs
        {
            Stage = CreateMagicDeckDocumentStageEnum.SaveToDocument,
            Percent = e.Percent,
            ErrorMessage = e.ErrorMessage
        });
    }
}
