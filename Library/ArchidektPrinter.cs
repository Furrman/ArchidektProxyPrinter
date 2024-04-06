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


    public async Task GenerateWord(int? deckId = null, string? inputFilePath = null, string? outputPath = null, string? outputFileName = null, bool saveImages = false)
    {
        if (deckId != 0) await GenerateWord(deckId!.Value, outputPath, outputFileName, saveImages);
        else if (inputFilePath != null) await GenerateWord(inputFilePath, outputPath, outputFileName, saveImages);
        else throw new ArgumentException("DeckId has to be bigger than 0 or WordFilePath has to be corrected");
    }

    public async Task GenerateWord(int deckId, string? outputPath = null, string? outputFileName = null, bool saveImages = false)
    {
        var deckDetails = await _deckService.GetDeckWithCardPrintDetails(deckId);
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


    private async Task GenerateWord(DeckDetailsDTO deckDetails, string? outputPath = null, string? outputFileName = null, bool saveImages = false)
    {
        var outputFolder = _fileManager.CreateOutputFolder(outputPath);
        var wordFilePath = _fileManager.ReturnCorrectWordFilePath(outputPath, outputFileName);

        double step = 1;
        double count = deckDetails.Cards.SelectMany(c => c.Value.ImageUrls).Count();
        if (count == 0)
        {
            RaiseError("Empty deck list");
            return;
        }

        await _wordGenerator.GenerateWord(wordFilePath, async (doc) =>
        {
            var paragraph = doc.AddParagraph();
            foreach (var card in deckDetails.Cards)
            {
                foreach (var entry in card.Value.ImageUrls)
                {
                    var imageContent = await _deckService.GetImage(entry.Value);
                    if (imageContent == null)
                    {
                        step = UpdateStep(step, count);
                        continue;
                    }
                    if (saveImages)
                    {
                        await _fileManager.CreateImageFile(imageContent, outputFolder, entry.Key);
                    }

                    _wordGenerator.AddImageToWord(paragraph, entry.Key, imageContent, card.Value.Quantity);
                    step = UpdateStep(step, count);
                }
            }
        });
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
