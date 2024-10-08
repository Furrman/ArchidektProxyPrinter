using Microsoft.Extensions.Logging;

using FluentAssertions;
using Moq;

using Domain.Services;

namespace UnitTests.Domain.Services;

public class LanguageServiceTests
{
    private readonly Mock<ILogger<LanguageService>> _loggerMock;
    private readonly LanguageService _service;

    public LanguageServiceTests()
    {
        _loggerMock = new Mock<ILogger<LanguageService>>();
        _service = new LanguageService(_loggerMock.Object);
    }

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
        // Act
        bool result = _service.IsValidLanguage(languageCode);

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
        // Act
        bool result = _service.IsValidLanguage(languageCode);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidLanguage_NullLanguageCode_ReturnsFalse()
    {
        // Act
        bool result = _service.IsValidLanguage(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AvailableLanguages_ReturnsCommaSeparatedLanguages()
    {
        // Arrange
        string expectedLanguages = "en, es, fr, de, it, pt, ja, ko, zhs, zht";

        // Act
        string availableLanguages = _service.AvailableLanguages;

        // Assert
        availableLanguages.Should().Be(expectedLanguages);
    }
}