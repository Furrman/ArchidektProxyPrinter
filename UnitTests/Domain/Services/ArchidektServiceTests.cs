using Microsoft.Extensions.Logging;

using FluentAssertions;
using Moq;

using Domain.Clients;
using Domain.Services;
using Domain.Models.DTO.Archidekt;

namespace UnitTests.Domain.Services;

public class ArchidektServiceTests
{
    private readonly Mock<IArchidektApiClient> _archidektApiClientMock;
    private readonly Mock<ILogger<ArchidektService>> _loggerMock;
    private readonly ArchidektService _service;

    public ArchidektServiceTests()
    {
        _archidektApiClientMock = new Mock<IArchidektApiClient>();
        _loggerMock = new Mock<ILogger<ArchidektService>>();
        _service = new ArchidektService(_archidektApiClientMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData("https://archidekt.com/api/decks/123/", 123)]
    [InlineData("https://archidekt.com/decks/456/", 456)]
    public void TryExtractDeckIdFromUrl_ValidUrl_ReturnsTrueAndExtractedDeckId(string url, int expectedDeckId)
    {
        // Act
        bool result = _service.TryExtractDeckIdFromUrl(url, out int deckId);

        // Assert
        result.Should().BeTrue();
        deckId.Should().Be(expectedDeckId);
    }

    [Theory]
    [InlineData("https://archidekt.com/api/decks/abc/")]
    [InlineData("https://archidekt.com/decks/xyz/")]
    [InlineData("https://archidekt.com/")]
    public void TryExtractDeckIdFromUrl_InvalidUrl_ReturnsFalse(string url)
    {
        // Act
        bool result = _service.TryExtractDeckIdFromUrl(url, out int deckId);

        // Assert
        result.Should().BeFalse();
        deckId.Should().Be(0);
    }

    [Fact]
    public async Task RetrieveDeckFromWeb_WithValidDeckId_ReturnsDeckDetails()
    {
        // Arrange
        string deckUrl = "https://archidekt.com/decks/123456/test";
        var deckDto = new DeckDTO("Test Deck",
        [
            new DeckCardDTO(new CardDTO(new OracleCardDTO("Card 1"), new EditionDTO()), 2),
            new DeckCardDTO(new CardDTO(new OracleCardDTO("Card 2"), new EditionDTO()), 3)
        ]);

        _archidektApiClientMock.Setup(mock => mock.GetDeck(It.IsAny<int>())).ReturnsAsync(deckDto);

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Deck");
        result.Cards.Should().HaveCount(2);
        result.Cards[0].Name.Should().Be("Card 1");
        result.Cards[0].Quantity.Should().Be(2);
        result.Cards[1].Name.Should().Be("Card 2");
        result.Cards[1].Quantity.Should().Be(3);
    }

    [Fact]
    public async Task RetrieveDeckFromWeb_WithInvalidDeckId_ReturnsNull()
    {
        // Arrange
        string deckUrl = "https://archidekt.com/decks/123456/test";

        _archidektApiClientMock.Setup(mock => mock.GetDeck(It.IsAny<int>())).ReturnsAsync((DeckDTO?)null);

        // Act
        var result = await _service.RetrieveDeckFromWeb(deckUrl);

        // Assert
        result.Should().BeNull();
    }
}