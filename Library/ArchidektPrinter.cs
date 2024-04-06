using Library.Clients;
using Library.IO;
using Library.Models.DTO;
using Library.Models.Events;
using Library.Services;

namespace Library;

public class ArchidektPrinter(
    DeckService deckService,
    CardListFileParser fileParser,
    WordGenerator wordGenerator,
    FileManager fileManager
        )
{
    public event EventHandler<UpdateProgressEventArgs>? ProgressUpdate;

    private readonly DeckService _deckService = deckService;
    private readonly CardListFileParser _fileParser = fileParser;
    private readonly WordGenerator _wordGenerator = wordGenerator;
    private readonly FileManager _fileManager = fileManager;
    

    public async Task SaveImages(int deckId, string? outputPath)
    {
        var deckDetails = await _deckService.GetDeckWithCardPrintDetails(deckId);
        if (deckDetails is null)
        {
            RaiseError("Getting deck details returned error");
            return;
        }

        await SaveImages(deckDetails.Cards, outputPath);
    }

    public async Task SaveImages(string deckListFilePath, string? outputPath)
    {
        outputPath = _fileManager.CreateOutputFolder(outputPath);

        var cardList = _fileParser.GetCardList(deckListFilePath);
        await SaveImages(cardList, outputPath);
    }

    public async Task SaveImagesAndGenerateWord(int deckId, string? outputPath, string? wordFilePath)
    {
        var deckDetails = await _deckService.GetDeckWithCardPrintDetails(deckId);
        if (deckDetails is null)
        {
            RaiseError("Getting deck details returned error");
            return;
        }

        await SaveImagesAndGenerateWord(deckDetails.Cards, outputPath, wordFilePath, deckDetails.Name);
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
        var deckDetails = await _deckService.GetDeckWithCardPrintDetails(deckId);
        if (deckDetails is null)
        {
            RaiseError("Getting deck details returned error");
            return;
        }

        await GenerateWord(deckDetails.Cards, wordFilePath, deckDetails.Name);
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

        double step = 1; 
        double count = cardList.SelectMany(c => c.Value.ImageUrls).Count();
        if (count == 0)
        {
            RaiseError("Empty deck list");
            return;
        }

        await _wordGenerator.GenerateWord(wordFilePath, async (doc) =>
        {
            var paragraph = doc.AddParagraph();
            foreach (var card in cardList)
            {
                foreach (var entry in card.Value.ImageUrls)
                {
                    var imageContent = await _deckService.GetImage(entry.Value);
                    if (imageContent == null)
                    {
                        step = UpdateStep(step, count);
                        continue;
                    }
                    _wordGenerator.AddImageToWord(paragraph, entry.Key, imageContent, card.Value.Quantity);
                    step = UpdateStep(step, count);
                }
            }
        });
    }

    private async Task SaveImages(Dictionary<string, CardEntryDTO> cardList, string? outputPath)
    {
        outputPath = _fileManager.CreateOutputFolder(outputPath);

        await _deckService.DownloadCards(cardList, outputPath!);
    }

    private async Task SaveImagesAndGenerateWord(Dictionary<string, CardEntryDTO> cardList, string? imageOutputPath, string? wordFilePath, string? deckName = null)
    {
        imageOutputPath = _fileManager.CreateOutputFolder(imageOutputPath);
        wordFilePath = _fileManager.ReturnCorrectFilePath(wordFilePath, deckName);

        await _deckService.DownloadCards(cardList, imageOutputPath!);
        _wordGenerator.GenerateWord(imageOutputPath!, wordFilePath!);
    }

    private void RaiseError(string? errorMessage = null)
    {
        ProgressUpdate?.Invoke(this, new UpdateProgressEventArgs
        {
            ErrorMessage = errorMessage
        });
    }

    private double UpdateStep(double step, double count)
    {
        step++;
        var percent = step / count * 100;
        ProgressUpdate?.Invoke(this, new UpdateProgressEventArgs
        {
            Percent = percent
        });
        return step;
    }
}
