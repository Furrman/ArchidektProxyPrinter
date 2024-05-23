using Microsoft.Extensions.Logging;

using Moq;

using Library.Clients;
using Library.IO;
using Library.Models.DTO;
using Library.Services;

namespace UnitTests.Library.Services;

public class WordGeneratorServiceTests
{
    [Fact]
    public async Task GenerateWord_ValidDeckDetails_SaveWordDocument()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<WordGeneratorService>>();
        var scryfallClientMock = new Mock<IScryfallApiClient>();
        var wordDocumentWrapperMock = new Mock<IWordDocumentWrapper>();
        var fileManagerMock = new Mock<IFileManager>();
        fileManagerMock.Setup(f => f.CreateOutputFolder(It.IsAny<string?>())).Returns("output");
        fileManagerMock.Setup(f => f.ReturnCorrectWordFilePath(It.IsAny<string?>(), It.IsAny<string>())).Returns((string path, string deckName) => path + deckName + ".docx");

        var service = new WordGeneratorService(loggerMock.Object, scryfallClientMock.Object, wordDocumentWrapperMock.Object, fileManagerMock.Object);
        var deck = new DeckDetailsDTO()
        {
            Name = "Deck",
            Cards =
            [
                new() { Name = "Card A", CardSides = [ new() { Name = "Card A" } ] },
            ]
        };
        string wordFilePath = "word.docx";

        // Act
        await service.GenerateWord(deck, wordFilePath);

        // Assert
        wordDocumentWrapperMock.Verify(w => w.Save(), Times.Once);
    }

    [Fact]
    public async Task GenerateWord_EmptyDeckDetails_DoesNotGenerateWordDocument()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<WordGeneratorService>>();
        var scryfallClientMock = new Mock<IScryfallApiClient>();
        var wordDocumentWrapperMock = new Mock<IWordDocumentWrapper>();
        var fileManagerMock = new Mock<IFileManager>();
        fileManagerMock.Setup(f => f.CreateOutputFolder(It.IsAny<string?>())).Returns("output");
        fileManagerMock.Setup(f => f.ReturnCorrectWordFilePath(It.IsAny<string?>(), It.IsAny<string>())).Returns((string path, string deckName) => path + deckName + ".docx");

        var service = new WordGeneratorService(loggerMock.Object, scryfallClientMock.Object, wordDocumentWrapperMock.Object, fileManagerMock.Object);
        DeckDetailsDTO deck = new();
        string wordFilePath = "word.docx";

        // Act
        await service.GenerateWord(deck, wordFilePath);

        // Assert
        wordDocumentWrapperMock.Verify(w => w.Save(), Times.Never);
    }

    [Fact]
    public async Task GenerateWord_SaveImagesEnabled_SaveImages()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<WordGeneratorService>>();
        var scryfallClientMock = new Mock<IScryfallApiClient>();
        var wordDocumentWrapperMock = new Mock<IWordDocumentWrapper>();
        var fileManagerMock = new Mock<IFileManager>();
        fileManagerMock.Setup(f => f.CreateOutputFolder(It.IsAny<string>())).Returns("output");
        fileManagerMock.Setup(f => f.ReturnCorrectWordFilePath(It.IsAny<string?>(), It.IsAny<string>())).Returns((string path, string deckName) => path + deckName + ".docx");

        var service = new WordGeneratorService(loggerMock.Object, scryfallClientMock.Object, wordDocumentWrapperMock.Object, fileManagerMock.Object);
        var deck = new DeckDetailsDTO()
        {
            Name = "Deck",
            Cards =
            [
                new() { Name = "Card A", CardSides = [ new() { Name = "Card A" } ] },
            ]
        };
        string wordFilePath = "word.docx";
        bool saveImages = true;

        // Act
        await service.GenerateWord(deck, wordFilePath, saveImages: saveImages);

        // Assert
        fileManagerMock.Verify(f => f.CreateImageFile(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GenerateWord_SaveImagesDisabled_DoesNotSaveImages()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<WordGeneratorService>>();
        var scryfallClientMock = new Mock<IScryfallApiClient>();
        var wordDocumentWrapperMock = new Mock<IWordDocumentWrapper>();
        var fileManagerMock = new Mock<IFileManager>();
        fileManagerMock.Setup(f => f.CreateOutputFolder(It.IsAny<string>())).Returns("output");
        fileManagerMock.Setup(f => f.ReturnCorrectWordFilePath(It.IsAny<string?>(), It.IsAny<string>())).Returns((string path, string deckName) => path + deckName + ".docx");

        var service = new WordGeneratorService(loggerMock.Object, scryfallClientMock.Object, wordDocumentWrapperMock.Object, fileManagerMock.Object);
        var deck = new DeckDetailsDTO()
        {
            Name = "Deck",
            Cards =
            [
                new() { Name = "Card A", CardSides = [ new() { Name = "Card A" } ] },
            ]
        };
        string wordFilePath = "word.docx";
        bool saveImages = false;

        // Act
        await service.GenerateWord(deck, wordFilePath, saveImages: saveImages);

        // Assert
        fileManagerMock.Verify(f => f.CreateImageFile(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}