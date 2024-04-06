using Library;
using Library.Services;
using Microsoft.Extensions.DependencyInjection;
using ConsoleApp.Configuration;

namespace ConsoleApp;

internal class Program
{
    static void Main(string[] args)
    {
        var serviceProvider = DependencyInjectionConfigurator.Setup();

        if (args.Length < 1)
        {
            Console.WriteLine(@"You have to provide at least one from this list:
            - path to exported deck
            - deck id
            - url to your deck.");
            return;
        }

        var deckService = serviceProvider.GetService<DeckService>()!;

        int deckId = 0;
        string? inputFilePath = null;
        if (Path.Exists(args[0]))
        {
            inputFilePath = args[0];
        }
        else
        {
            int.TryParse(args[0], out deckId);
            if (deckId == 0) deckService.TryExtractDeckIdFromUrl(args[0], out deckId);
        }
        if (deckId == 0)
        {
            Console.WriteLine("You have to specify correct ID of your deck in Archidekt or path to your card list exported from Archidekt.");
            return;
        }

        var outputDirectoryPath = args.Length > 1 ? args[1] : null;
        var wordFilePath = args.Length > 2 ? args[2] : null;

        var archidektPrinter = serviceProvider.GetService<ArchidektPrinter>()!;
        archidektPrinter.DownloadDeckProgress += DownloadDeckProgress;
        archidektPrinter.GenerateWordProgress += GenerateWordProgress;

        // Version with storing images and word
        //archidektPrinter.SaveImagesAndGenerateWord(deckId, inputFilePath, outputDirectoryPath, wordFilePath).Wait();
        // Version with saving just a word
        archidektPrinter.GenerateWord(deckId, outputDirectoryPath, wordFilePath).Wait();
    }

    private static void DownloadDeckProgress(object? sender, Library.Models.Events.DownloadDeckProgressEventArgs e)
    {
        PrintOutputToConsole(e.Percent, e.ErrorMessage);
    }

    private static void GenerateWordProgress(object? sender, Library.Models.Events.GenerateWordProgressEventArgs e)
    {
        PrintOutputToConsole(e.Percent, e.ErrorMessage);
    }

    private static void PrintOutputToConsole(double? percent, string? errorMessage)
    {
        if (percent is not null)
        {
            Console.WriteLine($"{percent:F1}%");
        }
        if (errorMessage is not null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorMessage);
            Console.ResetColor();
        }
    }
}