using Microsoft.Extensions.Logging;

using FluentAssertions;
using Moq;

using Domain.Clients;
using Domain.Models.DTO;
using Domain.Models.DTO.Scryfall;
using Domain.Services;
using Domain.IO;
using Domain.Constants;

namespace UnitTests.Domain.Services;

public class ScryfallServiceTests
{
    private readonly Mock<IScryfallClient> _scryfallClientMock;
    private readonly Mock<IFileManager> _fileManagerMock;
    private readonly Mock<ILogger<ScryfallService>> _loggerMock;
    private readonly ScryfallService _service;

    public ScryfallServiceTests()
    {
        _scryfallClientMock = new Mock<IScryfallClient>();
        _fileManagerMock = new Mock<IFileManager>();
        _loggerMock = new Mock<ILogger<ScryfallService>>();
        _service = new ScryfallService(_scryfallClientMock.Object, 
            _fileManagerMock.Object,
            _loggerMock.Object);
    }


    [Fact]
    public async Task DownloadCardSideImage_WhenImageUrlProvided_ShouldTryToDownloadImage()
    {
        // Arrange
        var imageUrl = "https://example.com/image.jpg";
        var folderPath = "/path/to/folder";
        var filename = "card";
        var quantity = 1;
        var imageBytes = new byte[] { 0x12, 0x34, 0x56, 0x78 };

        _scryfallClientMock.Setup(api => api.DownloadImage(imageUrl)).ReturnsAsync(imageBytes);

        // Act
        await _service.DownloadCardSideImage(imageUrl, folderPath, filename, quantity);

        // Assert
        _scryfallClientMock.Verify(api => api.DownloadImage(imageUrl), Times.Once);
    }
    
    [Fact]
    public async Task DownloadCardSideImage_WhenImageBytesAreNotNull_ShouldDownloadImage()
    {
        // Arrange
        var imageUrl = "https://example.com/image.jpg";
        var folderPath = "/path/to/folder";
        var filename = "card";
        var quantity = 1;
        var imageBytes = new byte[] { 0x12, 0x34, 0x56, 0x78 };

        _scryfallClientMock.Setup(api => api.DownloadImage(imageUrl)).ReturnsAsync(imageBytes);

        // Act
        await _service.DownloadCardSideImage(imageUrl, folderPath, filename, quantity);

        // Assert
        _fileManagerMock.Verify(fm => fm.CreateImageFile(imageBytes, folderPath, It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    public async Task DownloadCardSideImage_WhenImageBytesAreNull_ShouldNotDownloadImage()
    {
        // Arrange
        var imageUrl = "https://example.com/image.jpg";
        var folderPath = "/path/to/folder";
        var filename = "card";
        var quantity = 1;
        var imageBytes = null as byte[];

        _scryfallClientMock.Setup(api => api.DownloadImage(imageUrl)).ReturnsAsync(imageBytes);

        // Act
        await _service.DownloadCardSideImage(imageUrl, folderPath, filename, quantity);

        // Assert
        _fileManagerMock.Verify(fm => fm.CreateImageFile(imageBytes!, folderPath, It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCardImageLinks_WhenCardsAreProvided_ShouldSetCardImageLinks()
    {
        // Arrange
        List<CardEntryDTO> cards =
        [
            new() { Name = "Card 1" }
        ];

        _scryfallClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new CardSearchDTO(
                [
                    new CardDataDTO { Name = "Card 1", ImageUriData = new CardImageUriDTO("https://example.com/card1.jpg")}
                ]));

        // Act
        await _service.UpdateCardImageLinks(cards);

        // Assert
        _scryfallClientMock.Verify(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        cards[0].CardSides.First().ImageUrl.Should().Be("https://example.com/card1.jpg");
    }

    [Fact]
    public async Task UpdateCardImageLinks_WhenCardIsNotFound_ShouldNotUpdateCardImageLinks()
    {
        // Arrange
        List<CardEntryDTO> cards =
        [
            new() { Name = "Card 1" }
        ];

        _scryfallClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync((CardSearchDTO?)null);

        // Act
        await _service.UpdateCardImageLinks(cards);

        // Assert
        _scryfallClientMock.Verify(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        cards[0].CardSides.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateCardImageLinks_WhenCardIsDualSide_ShouldUpdateBothPagesCardImageLinks()
    {
        // Arrange
        List<CardEntryDTO> cards =
        [
            new() { Name = "Card 1" }
        ];

        _scryfallClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new CardSearchDTO(
                [
                    new CardDataDTO { Name = "Card 1", CardFaces =
                    [
                        new("Front", new CardImageUriDTO("https://example.com/card1.jpg")),
                        new("Back", new CardImageUriDTO("https://example.com/card1-back.jpg"))
                    ]}
                ]));

        // Act
        await _service.UpdateCardImageLinks(cards);

        // Assert
        _scryfallClientMock.Verify(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        cards[0].CardSides.Should().NotBeEmpty().And.HaveCount(2);
    }

    [Fact]
    public async Task UpdateCardImageLinks_WhenCardIsArt_ShouldUpdateFirstPageCardImageLink()
    {
        // Arrange
        List<CardEntryDTO> cards =
        [
            new() { Name = "Card 1", Art = true }
        ];

        _scryfallClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new CardSearchDTO(
                [
                    new CardDataDTO 
                    { 
                        Name = "Card 1", 
                        CardFaces =
                        [
                            new("Front", new CardImageUriDTO("https://example.com/card1.jpg")),
                            new("Back", new CardImageUriDTO("https://example.com/card1-back.jpg"))
                        ]
                    }
                ]));

        // Act
        await _service.UpdateCardImageLinks(cards);

        // Assert
        _scryfallClientMock.Verify(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        cards[0].CardSides.Should().NotBeEmpty()
            .And.HaveCount(1)
            .And.ContainSingle(cs => cs.ImageUrl == "https://example.com/card1.jpg");
    }

    [Fact]
    public async Task UpdateCardImageLinks_WhenTokenCopyNumberSpecified_ShouldAddTokens()
    {
        // Arrange
        List<CardEntryDTO> cards =
        [
            new() { Name = "Card 1" }
        ];

        _scryfallClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new CardSearchDTO(
                [
                    new CardDataDTO { Name = "Card 1", AllParts = [new CardPartDTO("Card 1 Token", ScryfallParts.TOKEN, "https://example.com/card1-token.jpg")] }
                ]));

        // Act
        await _service.UpdateCardImageLinks(cards, tokenCopies: 1);

        // Assert
        cards[0].Tokens.Should().HaveCount(1)
            .And.ContainSingle(cs => cs.Uri == "https://example.com/card1-token.jpg" && cs.Name == "Card 1 Token");
    }

    [Fact]
    public async Task UpdateCardImageLinks_WhenTokenCopyNumberNotSet_ShouldNotAddTokens()
    {
        // Arrange
        List<CardEntryDTO> cards =
        [
            new() { Name = "Card 1" }
        ];

        _scryfallClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new CardSearchDTO(
                [
                    new CardDataDTO { Name = "Card 1", AllParts = [new CardPartDTO("Card 1 Token", ScryfallParts.TOKEN, "https://example.com/card1-token.jpg")] }
                ]));

        // Act
        await _service.UpdateCardImageLinks(cards);

        // Assert
        cards[0].Tokens.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateCardImageLinks_WhenTokenCopyNumberSpecified_ShouldAddTokensSpecifiedNumberOfTimesToOtherCardsForPrinting()
    {
        // Arrange
        List<CardEntryDTO> cards =
        [
            new() { Name = "Card 1" }
        ];
        var tokenId = Guid.NewGuid();

        _scryfallClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new CardSearchDTO(
                [
                    new CardDataDTO { Name = "Card 1", AllParts = [new CardPartDTO("Card 1 Token", ScryfallParts.TOKEN, $"https://example.com/{tokenId}")] }
                ]));
        _scryfallClientMock.Setup(api => api.GetCard(tokenId))
            .ReturnsAsync(new CardDataDTO { Name = "Card 1 Token", ImageUriData = new CardImageUriDTO("https://example.com/card1-token.jpg") });

        // Act
        await _service.UpdateCardImageLinks(cards, tokenCopies: 3);

        // Assert
        cards.Should().Contain(c => c.Name == "Card 1 Token" && c.Quantity == 3 && c.CardSides.First().ImageUrl == "https://example.com/card1-token.jpg");
    }

    [Fact]
    public async Task UpdateCardImageLinks_WhenTokenCopyNumberNotSpecified_ShouldNotAddAnyTokensToOtherCardsForPrinting()
    {
        // Arrange
        List<CardEntryDTO> cards =
        [
            new() { Name = "Card 1" }
        ];
        var tokenId = Guid.NewGuid();

        _scryfallClientMock.Setup(api => api.SearchCard(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(new CardSearchDTO(
                [
                    new CardDataDTO { Name = "Card 1", AllParts = [new CardPartDTO("Card 1 Token", ScryfallParts.TOKEN, $"https://example.com/{tokenId}")] }
                ]));
        _scryfallClientMock.Setup(api => api.GetCard(tokenId))
            .ReturnsAsync(new CardDataDTO { Name = "Card 1 Token", ImageUriData = new CardImageUriDTO("https://example.com/card1-token.jpg") });

        // Act
        await _service.UpdateCardImageLinks(cards);

        // Assert
        cards.Should().NotContain(c => c.Name == "Card 1 Token" && c.Quantity == 3 && c.CardSides.First().ImageUrl == "https://example.com/card1-token.jpg");
    }
    
    [Fact]
    public async Task UpdateCardImageLinks_WhenTokenCopyNumberSpecifiedAndGroupingTokensSet_ShouldNotAddTheSameTokenMoreTimesThanSpecified()
    {
        // TODO
    }
}