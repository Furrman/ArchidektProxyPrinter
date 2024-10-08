using Microsoft.Extensions.Logging;

using FluentAssertions;
using Moq;

using Domain.Clients;
using Domain.Models.DTO;
using Domain.Models.DTO.Scryfall;
using Domain.Services;
using Domain.IO;

namespace UnitTests.Domain.Services;

public class ScryfallServiceTests
{
    private readonly Mock<IScryfallApiClient> _scryfallApiClientMock;
    private readonly Mock<IFileManager> _fileManagerMock;
    private readonly Mock<ILogger<ScryfallService>> _loggerMock;
    private readonly ScryfallService _service;

    public ScryfallServiceTests()
    {
        _scryfallApiClientMock = new Mock<IScryfallApiClient>();
        _fileManagerMock = new Mock<IFileManager>();
        _loggerMock = new Mock<ILogger<ScryfallService>>();
        _service = new ScryfallService(_scryfallApiClientMock.Object, 
            _fileManagerMock.Object,
            _loggerMock.Object);
    }


    [Fact]
    public async Task DownloadCardSideImage_ShouldTryToDownloadImage_WhenImageUrlProvided()
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
        _fileManagerMock.Verify(fm => fm.CreateImageFile(imageBytes, folderPath, It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    public async Task DownloadCardSideImage_ShouldNotDownloadImage_WhenImageBytesAreNull()
    {
        // Arrange
        var imageUrl = "https://example.com/image.jpg";
        var folderPath = "/path/to/folder";
        var filename = "card";
        var quantity = 1;
        var imageBytes = null as byte[];

        _scryfallApiClientMock.Setup(api => api.DownloadImage(imageUrl)).ReturnsAsync(imageBytes);

        // Act
        await _service.DownloadCardSideImage(imageUrl, folderPath, filename, quantity);

        // Assert
        _fileManagerMock.Verify(fm => fm.CreateImageFile(imageBytes!, folderPath, It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCardImageLinks_ShouldSetCardImageLinks_WhenCardsAreProvided()
    {
        // Arrange
        List<CardEntryDTO> cards =
        [
            new() { Name = "Card 1" }
        ];

        _scryfallApiClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new CardSearchDTO(
                [
                    new CardDataDTO { Name = "Card 1", ImageUris = new CardImageUriDTO("https://example.com/card1.jpg")}
                ]));

        // Act
        await _service.UpdateCardImageLinks(cards);

        // Assert
        _scryfallApiClientMock.Verify(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        cards[0].CardSides.First().ImageUrl.Should().Be("https://example.com/card1.jpg");
    }

    [Fact]
    public async Task UpdateCardImageLinks_ShouldNotUpdateCardImageLinks_WhenCardIsNotFound()
    {
        // Arrange
        List<CardEntryDTO> cards =
        [
            new() { Name = "Card 1" }
        ];

        _scryfallApiClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync((CardSearchDTO?)null);

        // Act
        await _service.UpdateCardImageLinks(cards);

        // Assert
        _scryfallApiClientMock.Verify(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        cards[0].CardSides.Should().BeEmpty();
    }
}