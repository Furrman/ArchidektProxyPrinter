using Microsoft.Extensions.Logging;
using FluentAssertions;
using Moq;
using Library.Services;

namespace UnitTests.Library.Services;

public class LanguageServiceTests
{
    [Theory]
    [InlineData("en")]
    [InlineData("es")]
    [InlineData("fr")]
    [InlineData("de")]
    [InlineData("it")]
    [InlineData("pt")]
    [InlineData("ja")]
    [InlineData("ko")]
    [InlineData("zhs")]
    [InlineData("zht")]
    public void IsValidLanguage_ValidLanguageCode_ReturnsTrue(string? languageCode)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LanguageService>>();
        var service = new LanguageService(loggerMock.Object);

        // Act
        bool result = service.IsValidLanguage(languageCode);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("fr-CA")]
    [InlineData("es-MX")]
    [InlineData("pt-BR")]
    public void IsValidLanguage_InvalidLanguageCode_ReturnsFalse(string? languageCode)
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LanguageService>>();
        var service = new LanguageService(loggerMock.Object);

        // Act
        bool result = service.IsValidLanguage(languageCode);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidLanguage_NullLanguageCode_ReturnsFalse()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LanguageService>>();
        var service = new LanguageService(loggerMock.Object);

        // Act
        bool result = service.IsValidLanguage(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AvailableLanguages_ReturnsCommaSeparatedLanguages()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LanguageService>>();
        var service = new LanguageService(loggerMock.Object);
        string expectedLanguages = "en, es, fr, de, it, pt, ja, ko, zhs, zht";

        // Act
        string availableLanguages = service.AvailableLanguages;

        // Assert
        availableLanguages.Should().Be(expectedLanguages);
    }
}