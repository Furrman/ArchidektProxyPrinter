using Library.Clients;
using Library.IO;
using Library.Models.DTO;
using Library.Models.Events;
using Microsoft.Extensions.Logging;

namespace Library;

public class ArchidektPrinter
{
    public event EventHandler<DownloadDeckProgressEventArgs>? DownloadDeckProgress;
    public event EventHandler<GenerateWordProgressEventArgs>? GenerateWordProgress;

    private readonly ILogger<ArchidektPrinter> _logger;
    private readonly ArchidektApiClient _archidektApiClient;
    private readonly ScryfallApiClient _scryfallApiClient;
    private readonly CardListFileParser _fileParser;
    private readonly WordGenerator _wordGenerator;
    private readonly FileManager _fileManager;

    public ArchidektPrinter(
        ILogger<ArchidektPrinter> logger,
        ArchidektApiClient archidektApiClient,
        ScryfallApiClient scryfallApiClient,
        CardListFileParser fileParser,
        WordGenerator wordGenerator,
        FileManager fileManager
        )
    {
        _logger = logger;
        _archidektApiClient = archidektApiClient;
        _scryfallApiClient = scryfallApiClient;
        _wordGenerator = wordGenerator;
        _fileParser = fileParser;
        _fileManager = fileManager;
    }


    public async Task SaveImages(int deckId, string? outputPath)
    {
        var cardList = await _archidektApiClient.GetCardList(deckId);
        await SaveImages(cardList, outputPath);
    }

    public async Task SaveImages(string deckListFilePath, string? outputPath)
    {
        outputPath = _fileManager.CreateOutputFolder(outputPath);

        var cardList = _fileParser.GetCardList(deckListFilePath);
        await SaveImages(cardList, outputPath);
    }

    public async Task SaveImagesAndGenerateWord(int deckId, string? outputPath, string? wordFilePath)
    {
        var cardList = await _archidektApiClient.GetCardList(deckId);
        var deckName = await _archidektApiClient.GetDeckName(deckId);
        await SaveImagesAndGenerateWord(cardList, outputPath, wordFilePath, deckName);
    }

    public async Task SaveImagesAndGenerateWord(string deckListFilePath, string? outputPath, string? wordFilePath)
    {
        var cardList = _fileParser.GetCardList(deckListFilePath);
        await SaveImagesAndGenerateWord(cardList, outputPath, wordFilePath);
    }

    public async Task SaveImagesAndGenerateWord(int deckId, string? deckListFilePath, string? outputPath, string? wordFilePath)
    {
        if (deckId != 0) await SaveImagesAndGenerateWord(deckId, outputPath, wordFilePath);
        else if (deckListFilePath != null) await SaveImagesAndGenerateWord(deckListFilePath, outputPath, wordFilePath);
        else throw new ArgumentException("DeckId has to be bigger than 0 or DeckListFilePath has to be correcet");
    }

    public async Task GenerateWord(int deckId, string? wordFilePath)
    {
        var cardList = await _archidektApiClient.GetCardList(deckId);
        var deckName = await _archidektApiClient.GetDeckName(deckId);
        await GenerateWord(cardList, wordFilePath, deckName);
    }

    public async Task GenerateWord(string deckListFilePath, string? wordFilePath)
    {
        var cardList = _fileParser.GetCardList(deckListFilePath);
        await GenerateWord(cardList, wordFilePath);
    }

    public async Task GenerateWord(int deckId, string? deckListFilePath, string? wordFilePath)
    {
        if (deckId != 0) await GenerateWord(deckId, wordFilePath);
        else if (deckListFilePath != null) await GenerateWord(deckListFilePath, wordFilePath);
        else throw new ArgumentException("DeckId has to be bigger than 0 or WordFilePath has to be correcet");
    }

    public void GenerateWordFromSavedImages(string imageFolderPath, string? wordFilePath)
    {
        if (!_fileManager.DirectoryExists(imageFolderPath))
        {
            throw new ArgumentException("ImageFolderPath has to be correct path to folder with card images");
        }
        wordFilePath = _fileManager.ReturnCorrectFilePath(wordFilePath);
        
        _wordGenerator.GenerateWord(imageFolderPath, wordFilePath!);
    }


    private async Task GenerateWord(Dictionary<string, CardEntryDTO> cardList, string? wordFilePath, string? deckName = null)
    {
        wordFilePath = _fileManager.ReturnCorrectFilePath(wordFilePath, deckName);
        _fileManager.CreateOutputFolder(Path.GetDirectoryName(wordFilePath));

        await _scryfallApiClient.UpdateCardImageLinks(cardList);

        double step = 0.0; 
        double progress = 0.0;
        double count = cardList.SelectMany(c => c.Value.ImageUrls).Count();
        if (count == 0)
        {
            UpdateWordProgress(errorMessage: "Empty deck list");
            return;
        }

        UpdateWordProgress(progress);

        await _wordGenerator.GenerateWord(wordFilePath, async (doc) =>
        {
            var paragraph = doc.AddParagraph();
            foreach (var card in cardList)
            {
                foreach (var entry in card.Value.ImageUrls)
                {
                    var imageContent = await _scryfallApiClient.GetImage(entry.Value);
                    if (imageContent == null)
                    {
                        step = UpdateWordStep(step, count, progress);
                        continue;
                    }
                    _wordGenerator.AddImageToWord(paragraph, entry.Key, imageContent, card.Value.Quantity);
                    step = UpdateWordStep(step, count, progress);
                }
            }
        });
    }

    private async Task SaveImages(Dictionary<string, CardEntryDTO> cardList, string? outputPath)
    {
        outputPath = _fileManager.CreateOutputFolder(outputPath);

        await _scryfallApiClient.DownloadCards(cardList, outputPath!);
    }

    private async Task SaveImagesAndGenerateWord(Dictionary<string, CardEntryDTO> cardList, string? imageOutputPath, string? wordFilePath, string? deckName = null)
    {
        imageOutputPath = _fileManager.CreateOutputFolder(imageOutputPath);
        wordFilePath = _fileManager.ReturnCorrectFilePath(wordFilePath, deckName);

        await _scryfallApiClient.DownloadCards(cardList, imageOutputPath!);
        _wordGenerator.GenerateWord(imageOutputPath!, wordFilePath!);
    }

    private void UpdateWordProgress(double? percent = null, string? errorMessage = null)
    {
        GenerateWordProgress?.Invoke(this, new GenerateWordProgressEventArgs
        {
            ErrorMessage = errorMessage,
            Percent = percent
        });
    }

    private double UpdateWordStep(double step, double count, double progress)
    {
        step++;
        progress = step / count * 100;
        UpdateWordProgress(progress);
        return step;
    }
}
