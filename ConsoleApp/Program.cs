﻿using Library;
using Library.Clients;
using Microsoft.Extensions.Logging;
using ConsoleApp.Configuration;

namespace ConsoleApp;

internal class Program
{
    static void Main(string[] args)
    {
        var loggerFactory = NLogConfigurator.SetupNLog();

        if (args.Length < 1)
        {
            Console.WriteLine(@"You have to provide at least:
            - path to exported deck
            - deck id
            - url to your deck.");
            return;
        }

        Dictionary<string, int> cardList;
        string filename;
        string outputDirectoryPath;
        var archidektClient = new ArchidektApiClient(loggerFactory.CreateLogger<ArchidektApiClient>());
        if (Path.Exists(args[0]))
        {
            var fileParser = new CardListFileParser(loggerFactory.CreateLogger<CardListFileParser>());
            cardList = fileParser.GetCardList(args[0].ToString());
            filename = Path.GetFileNameWithoutExtension(args[0]);
        }
        else if (int.TryParse(args[0], out int deckId) || 
            archidektClient.TryExtractDeckIdFromUrl(args[0], out deckId))
        {
            cardList = archidektClient.GetCardList(deckId).Result;
            filename = archidektClient.GetDeckName(deckId).Result;
        }
        else
        {
            Console.WriteLine("You have to specify ID of your deck in Archidekt or path to your card list exported from Archidekt.");
            return;
        }

        if (args.Length > 1 && Path.Exists(args[1]))
        {
            outputDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), args[1]);
        }
        else
        {
            outputDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), filename);
        }

        Console.WriteLine("Downloading images...");
        var scryFallClient = new ScryfallApiClient(loggerFactory.CreateLogger<ScryfallApiClient>());
        scryFallClient.DownloadCards(cardList, outputDirectoryPath).Wait();
        Console.WriteLine("Download completed!");

        Console.WriteLine("Creating word document...");
        var wordGenerator = new PrintableCardsWordGenerator(loggerFactory.CreateLogger<PrintableCardsWordGenerator>());
        wordGenerator.GenerateWord(outputDirectoryPath, outputDirectoryPath, filename: filename);
        Console.WriteLine("Creation completed!");
    }
}