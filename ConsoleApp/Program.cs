using Microsoft.Extensions.DependencyInjection;

using CoconoaApp = Cocona.CoconaLiteApp;
using CoconoaOptions = Cocona.OptionAttribute;

using ConsoleApp.Configuration;
using ConsoleApp.Helpers;
using Library;
using Library.Services;
using Library.Models.Events;

namespace ConsoleApp;

internal class Program
{
    static void Main(string[] args)
    {
        var serviceProvider = DependencyInjectionConfigurator.Setup();

        CoconoaApp.Run(([CoconoaOptions(Description = "Filepath to exported deck from Archidekt")] string? deckFilePath,
            [CoconoaOptions(Description = "ID of the deck in Archidekt")] int? deckId,
            [CoconoaOptions(Description = "URL link to deck in Archidekt")]string? deckUrl,
            [CoconoaOptions(Description = "Set language for all cards to print")] string? languageCode = null,
            [CoconoaOptions(Description = "Number of copy for each token")] int? tokenCopies = null,
            [CoconoaOptions(Description = "Print all tokens or reduce different version of the same token")] bool printAllTokens = false,
            [CoconoaOptions(Description = "Directory path to output file(s)")]string? outputPath = null,
            [CoconoaOptions(Description = "Filename of the output word file")]string? outputFileName = null,
            [CoconoaOptions(Description = "Flag to store original images in the same folder as output file")] bool storeOriginalImages = false) =>
        {
            if (deckFilePath is not null)
            {
                if (!Path.Exists(deckFilePath))
                {
                    ConsoleUtility.WriteErrorMessage("You have to specify correct PATH to your card list exported from Archidekt.");
                    return;
                }
            }
            else if (deckId is not null)
            {
                if (deckId <= 0)
                {
                    ConsoleUtility.WriteErrorMessage("You have to specify correct ID of your deck in Archidekt.");
                    return;
                }
            }
            else if (deckUrl is not null)
            {
                var archidektService = serviceProvider.GetService<IArchidektService>()!;
                if (!archidektService.TryExtractDeckIdFromUrl(deckUrl, out var urlDeckId) || urlDeckId <= 0)
                {
                    ConsoleUtility.WriteErrorMessage("You have to specify correct URL to your deck hosted by Archidekt.");
                    return;
                }
                deckId = urlDeckId;
            }
            else
            {
                ConsoleUtility.WriteErrorMessage(@"You have to provide at least one from this list:
                - path to exported deck
                - deck id
                - url to your deck.");
                return;
            }

            var languageService = serviceProvider.GetService<ILanguageService>()!;
            if (languageCode is not null && languageService.IsValidLanguage(languageCode) == false)
            {
                ConsoleUtility.WriteErrorMessage("You have to specify correct language code.");
                ConsoleUtility.WriteErrorMessage($"Language codes: {languageService.AvailableLanguages}");
                return;
            }

            if (tokenCopies is not null && tokenCopies <= 0)
            {
                ConsoleUtility.WriteErrorMessage("Number of copies for each token has to be greater than 0.");
                return;
            }
            if (tokenCopies is not null && tokenCopies > 100)
            {
                ConsoleUtility.WriteErrorMessage("Number of copies for each token has to be less than 100.");
                return;
            }

            var archidektPrinter = serviceProvider.GetService<IMTGProxyPrinter>()!;
            archidektPrinter.ProgressUpdate += UpdateProgressOnConsole;
            archidektPrinter.GenerateWord(deckId, 
                deckFilePath, 
                outputPath, 
                outputFileName, 
                languageCode,
                tokenCopies ?? 0,
                printAllTokens,
                storeOriginalImages).Wait();
        });
    }

    private static void UpdateProgressOnConsole(object? sender, UpdateProgressEventArgs e)
    {
        if (e.Percent is not null)
        {
            if (e.Percent == 0)
            {
                var stageInfo = e.Stage switch
                {
                    CreateMagicDeckDocumentStageEnum.GetDeckDetails => "(1/2) Get deck details",
                    CreateMagicDeckDocumentStageEnum.SaveToDocument => "(2/2) Download images",
                    _ => string.Empty
                };
                ConsoleUtility.WriteInNewLine(stageInfo);
            }
            ConsoleUtility.WriteProgressBar((int)e.Percent, true);
        }

        if (e.ErrorMessage is not null)
        {
            ConsoleUtility.WriteErrorMessage(e.ErrorMessage);
        }
    }
}