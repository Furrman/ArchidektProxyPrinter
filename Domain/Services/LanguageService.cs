using Microsoft.Extensions.Logging;

using Domain.Constants;

namespace Domain.Services;

/// <summary>
/// Represents a service for language-related operations.
/// </summary>
public interface ILanguageService
{
    /// <summary>
    /// Checks if a language code is valid.
    /// </summary>
    /// <param name="languageCode">The language code to validate.</param>
    /// <returns><c>true</c> if the language code is valid or null; otherwise, <c>false</c>.</returns>
    bool IsValidLanguage(string? languageCode);

    /// <summary>
    /// Gets the available languages in the application.
    /// It is taken from Scryfall API documentation to access cards in a specific language.
    /// </summary>
    string AvailableLanguages { get; }
}

public class LanguageService(ILogger<LanguageService> logger) : ILanguageService
{
    private readonly ILogger<LanguageService> _logger = logger;
    private readonly HashSet<string> _languages = [
        LanguageCodes.ENGLISH_CODE, 
        LanguageCodes.SPANISH_CODE, 
        LanguageCodes.FRENCH_CODE, 
        LanguageCodes.GERMAN_CODE, 
        LanguageCodes.ITALIAN_CODE, 
        LanguageCodes.PORTUGUESE_CODE, 
        LanguageCodes.JAPANESE_CODE, 
        LanguageCodes.KOREAN_CODE,
        LanguageCodes.CHINESE_SIMPLIFIED_CODE, 
        LanguageCodes.CHINESE_TRADITIONAL_CODE_CODE
    ];

    public bool IsValidLanguage(string? languageCode) => languageCode is not null && _languages.Contains(languageCode);

    public string AvailableLanguages => string.Join(", ", _languages);
}