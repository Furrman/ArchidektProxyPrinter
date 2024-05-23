using FluentAssertions;
using Library.Services;

namespace UnitTests.Library.Services;

public class ArchidektServiceTests
{
    [Theory]
    [InlineData("https://archidekt.com/api/decks/123/", 123)]
    [InlineData("https://archidekt.com/decks/456/", 456)]
    public void TryExtractDeckIdFromUrl_ValidUrl_ReturnsTrueAndExtractedDeckId(string url, int expectedDeckId)
    {
        // Arrange
        var service = new ArchidektService();

        // Act
        bool result = service.TryExtractDeckIdFromUrl(url, out int deckId);

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
        // Arrange
        var service = new ArchidektService();

        // Act
        bool result = service.TryExtractDeckIdFromUrl(url, out int deckId);

        // Assert
        result.Should().BeFalse();
        deckId.Should().Be(0);
    }
}