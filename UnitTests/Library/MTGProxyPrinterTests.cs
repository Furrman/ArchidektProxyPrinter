using FluentAssertions;
using Moq;

using Library;
using Library.IO;
using Library.Services;
using Library.Models.DTO;

namespace UnitTests.Library;

public class MTGProxyPrinterTests
{
    private Mock<IArchidektService> _archidektServiceMock;
    private Mock<IScryfallService> _scryfallServiceMock;
    private Mock<ICardListFileParser> _fileParserMock;
    private Mock<IWordGeneratorService> _wordGeneratorServiceMock;
    
    private MTGProxyPrinter _proxyPrinter;

    public MTGProxyPrinterTests()
    {
        _archidektServiceMock = new Mock<IArchidektService>();
        _scryfallServiceMock = new Mock<IScryfallService>();
        _fileParserMock = new Mock<ICardListFileParser>();
        _wordGeneratorServiceMock = new Mock<IWordGeneratorService>();

        _proxyPrinter = new MTGProxyPrinter(
            _archidektServiceMock.Object,
            _scryfallServiceMock.Object,
            _fileParserMock.Object,
            _wordGeneratorServiceMock.Object
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

        _archidektServiceMock.Setup(x => x.GetDeckOnline(deckId))
            .ReturnsAsync(new DeckDetailsDTO());

        // Act
        await _proxyPrinter.GenerateWord(deckId, null, outputPath, outputFileName, languageCode, tokenCopies, printAllTokens, saveImages);

        // Assert
        _archidektServiceMock.Verify(x => x.GetDeckOnline(deckId), Times.Once);
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
        _scryfallServiceMock.Verify(x => x.UpdateCardImageLinks(It.IsAny<List<CardEntryDTO>>(), languageCode, tokenCopies, printAllTokens), Times.Once);
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