using Moq;
using Library;
using Library.IO;
using Library.Services;
using Library.Models.DTO;
using FluentAssertions;

namespace UnitTests.Library;

public class MTGProxyPrinterTests
{
    private Mock<IMagicCardService> _magicCardServiceMock;
    private Mock<IWordGeneratorService> _wordGeneratorServiceMock;
    private Mock<IFileManager> _fileManagerMock;
    private Mock<ICardListFileParser> _fileParserMock;
    private MTGProxyPrinter _proxyPrinter;

    public MTGProxyPrinterTests()
    {
        _magicCardServiceMock = new Mock<IMagicCardService>();
        _wordGeneratorServiceMock = new Mock<IWordGeneratorService>();
        _fileManagerMock = new Mock<IFileManager>();
        _fileParserMock = new Mock<ICardListFileParser>();

        _proxyPrinter = new MTGProxyPrinter(
            _magicCardServiceMock.Object,
            _wordGeneratorServiceMock.Object,
            _fileManagerMock.Object,
            _fileParserMock.Object
        );
    }

    [Fact]
    public async Task GenerateWord_WithDeckId_CallsGenerateWordFromDeckOnline()
    {
        // Arrange
        int deckId = 123;
        string outputPath = "/path/to/output";
        string outputFileName = "output.docx";
        string languageCode = "en";
        int tokenCopies = 2;
        bool printAllTokens = true;
        bool saveImages = true;

        _magicCardServiceMock.Setup(x => x.GetOnlineDeckWithCardPrintDetails(deckId, languageCode, tokenCopies, printAllTokens))
            .ReturnsAsync(new DeckDetailsDTO());

        // Act
        await _proxyPrinter.GenerateWord(deckId, null, outputPath, outputFileName, languageCode, tokenCopies, printAllTokens, saveImages);

        // Assert
        _magicCardServiceMock.Verify(x => x.GetOnlineDeckWithCardPrintDetails(deckId, languageCode, tokenCopies, printAllTokens), Times.Once);
        _wordGeneratorServiceMock.Verify(x => x.GenerateWord(It.IsAny<DeckDetailsDTO>(), It.IsAny<string>(), It.IsAny<string>(), saveImages), Times.Once);
    }

    [Fact]
    public async Task GenerateWord_WithInputFilePath_CallsGenerateWordFromDeckInFile()
    {
        // Arrange
        string inputFilePath = "/path/to/input.txt";
        string outputPath = "/path/to/output";
        string outputFileName = "output.docx";
        string languageCode = "en";
        int tokenCopies = 2;
        bool printAllTokens = true;
        bool saveImages = true;

        _fileParserMock.Setup(x => x.GetDeckFromFile(inputFilePath))
            .Returns(new DeckDetailsDTO());

        // Act
        await _proxyPrinter.GenerateWord(null, inputFilePath, outputPath, outputFileName, languageCode, tokenCopies, printAllTokens, saveImages);

        // Assert
        _fileParserMock.Verify(x => x.GetDeckFromFile(inputFilePath), Times.Once);
        _magicCardServiceMock.Verify(x => x.UpdateCardImageLinks(It.IsAny<List<CardEntryDTO>>(), languageCode, tokenCopies, printAllTokens), Times.Once);
        _wordGeneratorServiceMock.Verify(x => x.GenerateWord(It.IsAny<DeckDetailsDTO>(), It.IsAny<string>(), It.IsAny<string>(), saveImages), Times.Once);
    }

    [Fact]
    public void GenerateWord_WithInvalidArguments_ThrowsArgumentException()
    {
        // Arrange
        int? deckId = null;
        string? inputFilePath = null;
        string outputPath = "/path/to/output";
        string outputFileName = "output.docx";
        string languageCode = "en";
        int tokenCopies = 2;
        bool printAllTokens = true;
        bool saveImages = true;

        // Act & Assert
        Func<Task> act = async () => await _proxyPrinter.GenerateWord(deckId, inputFilePath, outputPath, outputFileName, languageCode, tokenCopies, printAllTokens, saveImages);
        act.Should().ThrowAsync<ArgumentException>().WithMessage("Wrong input parameters to download deck.");
    }
}