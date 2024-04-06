﻿using Library;
using Microsoft.Extensions.DependencyInjection;
using ConsoleApp.Configuration;
using System.Text.RegularExpressions;

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

        var archidektPrinter = serviceProvider.GetService<ArchidektPrinter>()!;

        int deckId = 0;
        string? inputFilePath = null;
        if (Path.Exists(args[0]))
        {
            inputFilePath = args[0];
        }
        else
        {
            int.TryParse(args[0], out deckId);
            if (deckId == 0) archidektPrinter.TryExtractDeckIdFromUrl(args[0], out deckId);
        }
        if (deckId == 0)
        {
            Console.WriteLine("You have to specify correct ID of your deck in Archidekt or path to your card list exported from Archidekt.");
            return;
        }

        var outputDirectoryPath = args.Length > 1 ? args[1] : null;
        var wordFilePath = args.Length > 2 ? args[2] : null;

        archidektPrinter.SaveImagesAndGenerateWord(deckId, inputFilePath, outputDirectoryPath, wordFilePath).Wait();
    }
}