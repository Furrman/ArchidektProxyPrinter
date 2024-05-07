using Microsoft.Extensions.Logging;

using Moq;

namespace Library.IO.Tests;

public class CardListFileParserTest
{
    [Fact]
    public void GetDeckFromFile_CorrectFilePath_ReturnsDeckWithNameEqualFilename()
    {
        // Arrange
        var filePath = "path/to/file.txt";
        var loggerMock = new Mock<ILogger<CardListFileParser>>();
        var fileManagerMock = new Mock<IFileManager>();
        fileManagerMock.Setup(fm => fm.GetFilename(filePath)).Returns("file");
        fileManagerMock.Setup(fm => fm.GetLinesFromTextFile(filePath)).Returns([]);
        var parser = new CardListFileParser(loggerMock.Object, fileManagerMock.Object);

        // Act
        var result = parser.GetDeckFromFile(filePath);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("file", result.Name);
        Assert.Empty(result.Cards);
    }

    [Fact]
    public void GetDeckFromFile_QuantityWithoutX_ReturnsDeckDetailsDTOWithParsedCard()
    {
        // Arrange
        var filePath = "path/to/file.txt";
        var loggerMock = new Mock<ILogger<CardListFileParser>>();
        var fileManagerMock = new Mock<IFileManager>();
        fileManagerMock.Setup(fm => fm.GetFilename(filePath)).Returns("file");
        fileManagerMock.Setup(fm => fm.GetLinesFromTextFile(filePath)).Returns(
        [
            "Card A"
        ]);
        var parser = new CardListFileParser(loggerMock.Object, fileManagerMock.Object);

        // Act
        var result = parser.GetDeckFromFile(filePath);

        // Assert
        var cardA = result.Cards[0];
        Assert.Equal("Card A", cardA.Name);
        Assert.Equal(1, cardA.Quantity);
        Assert.Null(cardA.ExpansionCode);
        Assert.False(cardA.Foil);
        Assert.False(cardA.Etched);
    }

    [Fact]
    public void GetDeckFromFile_QuantityWithX_ReturnsDeckDetailsDTOWithParsedCard()
    {
        // Arrange
        var filePath = "path/to/file.txt";
        var loggerMock = new Mock<ILogger<CardListFileParser>>();
        var fileManagerMock = new Mock<IFileManager>();
        fileManagerMock.Setup(fm => fm.GetFilename(filePath)).Returns("file");
        fileManagerMock.Setup(fm => fm.GetLinesFromTextFile(filePath)).Returns(
        [
            "2x Card B"
        ]);
        var parser = new CardListFileParser(loggerMock.Object, fileManagerMock.Object);

        // Act
        var result = parser.GetDeckFromFile(filePath);

        // Assert
        var cardB = result.Cards[0];
        Assert.Equal("Card B", cardB.Name);
        Assert.Equal(2, cardB.Quantity);
        Assert.Null(cardB.ExpansionCode);
        Assert.False(cardB.Foil);
        Assert.False(cardB.Etched);
    }

    [Fact]
    public void GetDeckFromFile_CardName_ReturnsDeckDetailsDTOWithParsedCard()
    {
        // Arrange
        var filePath = "path/to/file.txt";
        var loggerMock = new Mock<ILogger<CardListFileParser>>();
        var fileManagerMock = new Mock<IFileManager>();
        fileManagerMock.Setup(fm => fm.GetFilename(filePath)).Returns("file");
        fileManagerMock.Setup(fm => fm.GetLinesFromTextFile(filePath)).Returns(
        [
            "1x Card C (EXP)"
        ]);
        var parser = new CardListFileParser(loggerMock.Object, fileManagerMock.Object);

        // Act
        var result = parser.GetDeckFromFile(filePath);

        // Assert
        var cardC = result.Cards[0];
        Assert.Equal("Card C", cardC.Name);
        Assert.Equal(1, cardC.Quantity);
        Assert.Equal("EXP", cardC.ExpansionCode);
        Assert.False(cardC.Foil);
        Assert.False(cardC.Etched);
    }

    [Fact]
    public void GetDeckFromFile_ExpansionProvided_ReturnsDeckDetailsDTOWithParsedCard()
    {
        // Arrange
        var filePath = "path/to/file.txt";
        var loggerMock = new Mock<ILogger<CardListFileParser>>();
        var fileManagerMock = new Mock<IFileManager>();
        fileManagerMock.Setup(fm => fm.GetFilename(filePath)).Returns("file");
        fileManagerMock.Setup(fm => fm.GetLinesFromTextFile(filePath)).Returns(
        [
            "1x Card D *F*"
        ]);
        var parser = new CardListFileParser(loggerMock.Object, fileManagerMock.Object);

        // Act
        var result = parser.GetDeckFromFile(filePath);

        // Assert
        var cardD = result.Cards[0];
        Assert.Equal("Card D", cardD.Name);
        Assert.Equal(1, cardD.Quantity);
        Assert.Null(cardD.ExpansionCode);
        Assert.True(cardD.Foil);
        Assert.False(cardD.Etched);
    }

    [Fact]
    public void GetDeckFromFile_ExpansionNotProvided_ReturnsDeckDetailsDTOWithParsedCard()
    {
        // Arrange
        var filePath = "path/to/file.txt";
        var loggerMock = new Mock<ILogger<CardListFileParser>>();
        var fileManagerMock = new Mock<IFileManager>();
        fileManagerMock.Setup(fm => fm.GetFilename(filePath)).Returns("file");
        fileManagerMock.Setup(fm => fm.GetLinesFromTextFile(filePath)).Returns(
        [
            "1x Card E *E*"
        ]);
        var parser = new CardListFileParser(loggerMock.Object, fileManagerMock.Object);

        // Act
        var result = parser.GetDeckFromFile(filePath);

        // Assert
        var cardE = result.Cards[0];
        Assert.Equal("Card E", cardE.Name);
        Assert.Equal(1, cardE.Quantity);
        Assert.Null(cardE.ExpansionCode);
        Assert.False(cardE.Foil);
        Assert.True(cardE.Etched);
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
        Assert.Equal(4, result.Cards.Count);

        var cardF = result.Cards[0];
        Assert.Equal("Card F", cardF.Name);
        Assert.Equal(2, cardF.Quantity);
        Assert.Null(cardF.ExpansionCode);
        Assert.False(cardF.Foil);
        Assert.False(cardF.Etched);

        // Assert Card G
        var cardG = result.Cards[1];
        Assert.Equal("Card G", cardG.Name);
        Assert.Equal(1, cardG.Quantity);
        Assert.Equal("EXP", cardG.ExpansionCode);
        Assert.False(cardG.Foil);
        Assert.False(cardG.Etched);

        // Assert Card H
        var cardH = result.Cards[2];
        Assert.Equal("Card H", cardH.Name);
        Assert.Equal(3, cardH.Quantity);
        Assert.Null(cardH.ExpansionCode);

        // Assert Card I
        var cardI = result.Cards[3];
        Assert.Equal("Card I", cardI.Name);
        Assert.Equal(1, cardI.Quantity);
        Assert.Null(cardI.ExpansionCode);
        Assert.False(cardI.Foil);
        Assert.True(cardI.Etched);
    }
}
