using Microsoft.Extensions.Logging;

using FluentAssertions;
using Moq;

using Domain.Clients;
using Domain.Models.DTO;
using Domain.Models.DTO.Scryfall;
using Domain.Services;

namespace UnitTests.Domain.Services;

public class ScryfallServiceTests
{
    private readonly Mock<IScryfallApiClient> _scryfallApiClientMock;
    private readonly Mock<ILogger<ScryfallService>> _loggerMock;
    private readonly ScryfallService _service;

    public ScryfallServiceTests()
    {
        _scryfallApiClientMock = new Mock<IScryfallApiClient>();
        _loggerMock = new Mock<ILogger<ScryfallService>>();
        _service = new ScryfallService(_scryfallApiClientMock.Object, _loggerMock.Object);
    }
    
    [Fact]
    public async Task DownloadCardSideImage_ShouldDownloadImage_WhenImageBytesAreNotNull()
    {
        // Arrange
        var imageUrl = "https://example.com/image.jpg";
        var folderPath = "/path/to/folder";
        var filename = "card";
        var quantity = 1;
        var imageBytes = new byte[] { 0x12, 0x34, 0x56, 0x78 };

        _scryfallApiClientMock.Setup(api => api.DownloadImage(imageUrl)).ReturnsAsync(imageBytes);

        // Act
        await _service.DownloadCardSideImage(imageUrl, folderPath, filename, quantity);

        // Assert
        _scryfallApiClientMock.Verify(api => api.DownloadImage(imageUrl), Times.Once);
    }

    [Fact]
    public async Task UpdateCardImageLinks_ShouldUpdateCardImageLinks_WhenCardsAreProvided()
    {
        // Arrange
        List<CardEntryDTO> cards =
        [
            new() { Name = "Card 1" },
            new() { Name = "Card 2" },
            new() { Name = "Card 3" }
        ];

        _scryfallApiClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new CardSearchDTO(
                [
                    new CardDataDTO { Name = "Card 1", ImageUris = new CardImageUriDTO("https://example.com/card1.jpg")},
                    new CardDataDTO { Name = "Card 2", ImageUris = new CardImageUriDTO("https://example.com/card2.jpg")},
                    new CardDataDTO { Name = "Card 3", ImageUris = new CardImageUriDTO("https://example.com/card3.jpg")}
                ]));

        // Act
        await _service.UpdateCardImageLinks(cards);

        // Assert
        _scryfallApiClientMock.Verify(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Exactly(3));
        cards[0].CardSides.First().ImageUrl.Should().Be("https://example.com/card1.jpg");
        cards[1].CardSides.First().ImageUrl.Should().Be("https://example.com/card2.jpg");
        cards[2].CardSides.First().ImageUrl.Should().Be("https://example.com/card3.jpg");
    }
}