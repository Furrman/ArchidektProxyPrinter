using CoconoaApp = Cocona.CoconaLiteApp;
using CoconoaOptions = Cocona.OptionAttribute;
using ConsoleApp.Configuration;
using Library;
using Library.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApp;

internal class Program
{
    static void Main(string[] args)
    {
        var serviceProvider = DependencyInjectionConfigurator.Setup();

        CoconoaApp.Run(([CoconoaOptions(Description = "Filepath to exported deck from Archidekt")] string? deckFilePath,
            [CoconoaOptions(Description = "ID of the deck in Archidekt")] int? deckId,
            [CoconoaOptions(Description = "URL link to deck in Archidekt")]string? deckUrl,
            [CoconoaOptions(Description = "Directory path to output file(s)")]string? outputPath,
            [CoconoaOptions(Description = "Filename of the output word file")]string? outputFileName,
            [CoconoaOptions(Description = "Flag to store original images in the same folder as output file")] bool storeOriginalImages = false) =>
        {
            if (deckFilePath is not null)
            {
                if (!Path.Exists(deckFilePath))
                {
                    Console.WriteLine("You have to specify correct PATH to your card list exported from Archidekt.");
                    return;
                }
            }
            else if (deckId is not null)
            {
                if (deckId <= 0)
                {
                    Console.WriteLine("You have to specify correct ID of your deck in Archidekt.");
                    return;
                }
            }
            else if (deckUrl is not null)
            {
                var magicCardService = serviceProvider.GetService<MagicCardService>()!;
                if (!magicCardService.TryExtractDeckIdFromUrl(args[0], out var urlDeckId) || urlDeckId <= 0)
                {
                    Console.WriteLine("You have to specify correct URL to your deck hosted by Archidekt.");
                    return;
                }
                deckId = urlDeckId;
            }
            else
            {
                Console.WriteLine(@"You have to provide at least one from this list:
                - path to exported deck
                - deck id
                - url to your deck.");
                return;
            }

            var archidektPrinter = serviceProvider.GetService<ArchidektPrinter>()!;
            archidektPrinter.ProgressUpdate += UpdateProgressOnConsole;
            archidektPrinter.GenerateWord(deckId, deckFilePath, outputPath, outputFileName, storeOriginalImages).Wait();
        });
    }

    private static void UpdateProgressOnConsole(object? sender, Library.Models.Events.UpdateProgressEventArgs e)
    {
        if (e.Percent is not null)
        {
            Console.WriteLine($"{e.Percent:F1}%");
        }
        if (e.ErrorMessage is not null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.ErrorMessage);
            Console.ResetColor();
        }
    }
}