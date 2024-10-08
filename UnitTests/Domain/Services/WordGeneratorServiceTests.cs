using Microsoft.Extensions.Logging;

using Moq;

using Domain.Clients;
using Domain.IO;
using Domain.Models.DTO;
using Domain.Services;

namespace UnitTests.Domain.Services;

public class WordGeneratorServiceTests
{
    private readonly Mock<ILogger<WordGeneratorService>> _loggerMock;
    private readonly Mock<IScryfallClient> _scryfallClientMock;
    private readonly Mock<IWordDocumentWrapper> _wordDocumentWrapperMock;
    private readonly Mock<IFileManager> _fileManagerMock;
    private readonly WordGeneratorService _service;

    public WordGeneratorServiceTests()
    {
        _loggerMock = new Mock<ILogger<WordGeneratorService>>();
        _scryfallClientMock = new Mock<IScryfallClient>();
        _wordDocumentWrapperMock = new Mock<IWordDocumentWrapper>();
        _fileManagerMock = new Mock<IFileManager>();

        _service = new WordGeneratorService(_loggerMock.Object, _scryfallClientMock.Object, _wordDocumentWrapperMock.Object, _fileManagerMock.Object);
    }

    [Fact]
    public async Task GenerateWord_ValidDeckDetails_SaveWordDocument()
    {
        // Arrange
        _fileManagerMock.Setup(f => f.CreateOutputFolder(It.IsAny<string?>())).Returns("output");
        _fileManagerMock.Setup(f => f.ReturnCorrectWordFilePath(It.IsAny<string?>(), It.IsAny<string>())).Returns((string path, string deckName) => path + deckName + ".docx");

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
        await _service.GenerateWord(deck, wordFilePath);

        // Assert
        _wordDocumentWrapperMock.Verify(w => w.Save(), Times.Once);
    }

    [Fact]
    public async Task GenerateWord_EmptyDeckDetails_DoesNotGenerateWordDocument()
    {
        // Arrange
        _fileManagerMock.Setup(f => f.CreateOutputFolder(It.IsAny<string?>())).Returns("output");
        _fileManagerMock.Setup(f => f.ReturnCorrectWordFilePath(It.IsAny<string?>(), It.IsAny<string>())).Returns((string path, string deckName) => path + deckName + ".docx");

        DeckDetailsDTO deck = new();
        string wordFilePath = "word.docx";

        // Act
        await _service.GenerateWord(deck, wordFilePath);

        // Assert
        _wordDocumentWrapperMock.Verify(w => w.Save(), Times.Never);
    }

    [Fact]
    public async Task GenerateWord_SaveImagesEnabled_SaveImages()
    {
        // Arrange
        _fileManagerMock.Setup(f => f.CreateOutputFolder(It.IsAny<string>())).Returns("output");
        _fileManagerMock.Setup(f => f.ReturnCorrectWordFilePath(It.IsAny<string?>(), It.IsAny<string>())).Returns((string path, string deckName) => path + deckName + ".docx");

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
        await _service.GenerateWord(deck, wordFilePath, saveImages: saveImages);

        // Assert
        _fileManagerMock.Verify(f => f.CreateImageFile(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GenerateWord_SaveImagesDisabled_DoesNotSaveImages()
    {
        // Arrange
        _fileManagerMock.Setup(f => f.CreateOutputFolder(It.IsAny<string>())).Returns("output");
        _fileManagerMock.Setup(f => f.ReturnCorrectWordFilePath(It.IsAny<string?>(), It.IsAny<string>())).Returns((string path, string deckName) => path + deckName + ".docx");

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
        await _service.GenerateWord(deck, wordFilePath, saveImages: saveImages);

        // Assert
        _fileManagerMock.Verify(f => f.CreateImageFile(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}