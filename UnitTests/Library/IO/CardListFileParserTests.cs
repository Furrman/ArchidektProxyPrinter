using Microsoft.Extensions.Logging;

using FluentAssertions;
using Moq;

using Domain.IO;

namespace UnitTests.Domain.IO;

public class CardListFileParserTest
{
    private readonly Mock<ILogger<CardListFileParser>> _loggerMock;
    private readonly Mock<IFileManager> _fileManagerMock;
    private readonly CardListFileParser _parser;

    public CardListFileParserTest()
    {
        _loggerMock = new Mock<ILogger<CardListFileParser>>();
        _fileManagerMock = new Mock<IFileManager>();
        _parser = new CardListFileParser(_loggerMock.Object, _fileManagerMock.Object);
    }

    [Fact]
    public void GetDeckFromFile_CorrectFilePath_ReturnsDeckWithNameEqualFilename()
    {
        // Arrange
        var filePath = "path/to/file.txt";
        _fileManagerMock.Setup(fm => fm.GetFilename(filePath)).Returns("file");
        _fileManagerMock.Setup(fm => fm.GetLinesFromTextFile(filePath)).Returns([]);

        // Act
        var result = _parser.GetDeckFromFile(filePath);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("file");
        result.Cards.Should().BeEmpty();
    }

    [Fact]
    public void GetDeckFromFile_QuantityWithoutX_ReturnsDeckDetailsDTOWithParsedCard()
    {
        // Arrange
        var filePath = "path/to/file.txt";
        _fileManagerMock.Setup(fm => fm.GetFilename(filePath)).Returns("file");
        _fileManagerMock.Setup(fm => fm.GetLinesFromTextFile(filePath)).Returns(
        [
            "Card A"
        ]);

        // Act
        var result = _parser.GetDeckFromFile(filePath);

        // Assert
        var cardA = result.Cards[0];
        cardA.Name.Should().Be("Card A");
        cardA.Quantity.Should().Be(1);
        cardA.ExpansionCode.Should().BeNull();
        cardA.Foil.Should().BeFalse();
        cardA.Etched.Should().BeFalse();
    }

    [Fact]
    public void GetDeckFromFile_QuantityWithX_ReturnsDeckDetailsDTOWithParsedCard()
    {
        // Arrange
        var filePath = "path/to/file.txt";
        _fileManagerMock.Setup(fm => fm.GetFilename(filePath)).Returns("file");
        _fileManagerMock.Setup(fm => fm.GetLinesFromTextFile(filePath)).Returns(
        [
            "2x Card B"
        ]);

        // Act
        var result = _parser.GetDeckFromFile(filePath);

        // Assert
        var cardB = result.Cards[0];
        cardB.Name.Should().Be("Card B");
        cardB.Quantity.Should().Be(2);
        cardB.ExpansionCode.Should().BeNull();
        cardB.Foil.Should().BeFalse();
        cardB.Etched.Should().BeFalse();
    }

    [Fact]
    public void GetDeckFromFile_CardName_ReturnsDeckDetailsDTOWithParsedCard()
    {
        // Arrange
        var filePath = "path/to/file.txt";
        _fileManagerMock.Setup(fm => fm.GetFilename(filePath)).Returns("file");
        _fileManagerMock.Setup(fm => fm.GetLinesFromTextFile(filePath)).Returns(
        [
            "1x Card C (EXP)"
        ]);

        // Act
        var result = _parser.GetDeckFromFile(filePath);

        // Assert
        var cardC = result.Cards[0];
        cardC.Name.Should().Be("Card C");
        cardC.Quantity.Should().Be(1);
        cardC.ExpansionCode.Should().Be("EXP");
        cardC.Foil.Should().BeFalse();
        cardC.Etched.Should().BeFalse();
    }

    [Fact]
    public void GetDeckFromFile_ExpansionProvided_ReturnsDeckDetailsDTOWithParsedCard()
    {
        // Arrange
        var filePath = "path/to/file.txt";
        _fileManagerMock.Setup(fm => fm.GetFilename(filePath)).Returns("file");
        _fileManagerMock.Setup(fm => fm.GetLinesFromTextFile(filePath)).Returns(
        [
            "1x Card D *F*"
        ]);

        // Act
        var result = _parser.GetDeckFromFile(filePath);

        // Assert
        var cardD = result.Cards[0];
        cardD.Name.Should().Be("Card D");
        cardD.Quantity.Should().Be(1);
        cardD.ExpansionCode.Should().BeNull();
        cardD.Foil.Should().BeTrue();
        cardD.Etched.Should().BeFalse();
    }

    [Fact]
    public void GetDeckFromFile_ExpansionNotProvided_ReturnsDeckDetailsDTOWithParsedCard()
    {
        // Arrange
        var filePath = "path/to/file.txt";
        _fileManagerMock.Setup(fm => fm.GetFilename(filePath)).Returns("file");
        _fileManagerMock.Setup(fm => fm.GetLinesFromTextFile(filePath)).Returns(
        [
            "1x Card E *E*"
        ]);

        // Act
        var result = _parser.GetDeckFromFile(filePath);

        // Assert
        var cardE = result.Cards[0];
        cardE.Name.Should().Be("Card E");
        cardE.Quantity.Should().Be(1);
        cardE.ExpansionCode.Should().BeNull();
        cardE.Foil.Should().BeFalse();
        cardE.Etched.Should().BeTrue();
    }

    [Fact]
    public void GetDeckFromFile_Multiline_ReturnsDeckDetailsDTOWithParsedCards()
    {
        // Arrange
        var filePath = "path/to/file.txt";
        var loggerMock = new Mock<ILogger<CardListFileParser>>();
        var fileManagerMock = new Mock<IFileManager>();
        fileManagerMock.Setup(fm => fm.GetFilename(filePath)).Returns("file");
        fileManagerMock.Setup(fm => fm.GetLinesFromTextFile(filePath)).Returns(
        [
            "2x Card F",
            "1x Card G (EXP)",
            "3 Card H *F*",
            "1 Card I *E*"
        ]);
        var parser = new CardListFileParser(loggerMock.Object, fileManagerMock.Object);

        // Act
        var result = parser.GetDeckFromFile(filePath);

        // Assert
        result.Cards.Count.Should().Be(4);

        var cardF = result.Cards[0];
        cardF.Name.Should().Be("Card F");
        cardF.Quantity.Should().Be(2);
        cardF.ExpansionCode.Should().BeNull();
        cardF.Foil.Should().BeFalse();
        cardF.Etched.Should().BeFalse();

        // Assert Card G
        var cardG = result.Cards[1];
        cardG.Name.Should().Be("Card G");
        cardG.Quantity.Should().Be(1);
        cardG.ExpansionCode.Should().Be("EXP");
        cardG.Foil.Should().BeFalse();
        cardG.Etched.Should().BeFalse();

        // Assert Card H
        var cardH = result.Cards[2];
        cardH.Name.Should().Be("Card H");
        cardH.Quantity.Should().Be(3);
        cardH.ExpansionCode.Should().BeNull();

        // Assert Card I
        var cardI = result.Cards[3];
        cardI.Name.Should().Be("Card I");
        cardI.Quantity.Should().Be(1);
        cardI.ExpansionCode.Should().BeNull();
        cardI.Foil.Should().BeFalse();
        cardI.Etched.Should().BeTrue();
    }
}
