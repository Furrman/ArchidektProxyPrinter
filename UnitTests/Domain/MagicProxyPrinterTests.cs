using FluentAssertions;
using Moq;

using Domain;
using Domain.IO;
using Domain.Services;
using Domain.Models.DTO;
using Domain.Factories;

namespace UnitTests.Domain;

public class MagicProxyPrinterTests
{
    private Mock<IArchidektService> _archidektServiceMock;
    private Mock<IDeckRetrieverFactory> _deckRetrieverFactory;
    private Mock<IScryfallService> _scryfallServiceMock;
    private Mock<ICardListFileParser> _fileParserMock;
    private Mock<IFileManager> _fileManagerMock;
    private Mock<IWordGeneratorService> _wordGeneratorServiceMock;
    
    private MagicProxyPrinter _proxyPrinter;

    public MagicProxyPrinterTests()
    {
        _archidektServiceMock = new Mock<IArchidektService>();
        _deckRetrieverFactory = new Mock<IDeckRetrieverFactory>();
        _scryfallServiceMock = new Mock<IScryfallService>();
        _fileParserMock = new Mock<ICardListFileParser>();
        _fileManagerMock = new Mock<IFileManager>();
        _wordGeneratorServiceMock = new Mock<IWordGeneratorService>();

        _proxyPrinter = new MagicProxyPrinter(
            _deckRetrieverFactory.Object,
            _scryfallServiceMock.Object,
            _fileParserMock.Object,
            _fileManagerMock.Object,
            _wordGeneratorServiceMock.Object
        );
    }

    [Fact]
    public async Task GenerateWord_WithDeckId_CallsGenerateWordFromDeckOnline()
    {
        // Arrange
        string deckUrl = "https://archidekt.com/decks/123456/test";
        string outputPath = "/path/to/output";
        string outputFileName = "output.docx";
        string languageCode = "en";
        int tokenCopies = 2;
        bool printAllTokens = true;
        bool saveImages = true;

        _deckRetrieverFactory.Setup(x => x.GetDeckRetriever(deckUrl))
            .Returns(_archidektServiceMock.Object);
        _archidektServiceMock.Setup(x => x.TryExtractDeckIdFromUrl(deckUrl, out It.Ref<int>.IsAny))
            .Returns(true);
        _archidektServiceMock.Setup(x => x.RetrieveDeckFromWeb(It.IsAny<string>()))
            .ReturnsAsync(new DeckDetailsDTO());

        // Act
        await _proxyPrinter.GenerateWord(deckUrl, null, outputPath, outputFileName, languageCode, tokenCopies, printAllTokens, saveImages);

        // Assert
        _archidektServiceMock.Verify(x => x.RetrieveDeckFromWeb(It.IsAny<string>()), Times.Once);
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

        _fileManagerMock.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);
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
        string? deckUrl = null;
        string? inputFilePath = null;
        string outputPath = "/path/to/output";
        string outputFileName = "output.docx";
        string languageCode = "en";
        int tokenCopies = 2;
        bool printAllTokens = true;
        bool saveImages = true;

        // Act
        Func<Task> act = async () => await _proxyPrinter.GenerateWord(deckUrl, inputFilePath, outputPath, outputFileName, languageCode, tokenCopies, printAllTokens, saveImages);
        
        // Assert
        act.Should().ThrowAsync<ArgumentException>().WithMessage("Wrong input parameters to download deck.");
    }
}