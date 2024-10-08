using Microsoft.Extensions.Logging;

using Domain.Constants;

namespace Domain.IO;

/// <summary>
/// Represents a file manager that provides methods for managing files and directories.
/// </summary>
public interface IFileManager
{
    /// <summary>
    /// Creates an image file with the specified content, folder path, and file name.
    /// </summary>
    /// <param name="content">The content of the image file.</param>
    /// <param name="folderPath">The path of the folder where the image file will be created.</param>
    /// <param name="fileName">The name of the image file.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateImageFile(byte[] content, string folderPath, string fileName);

    /// <summary>
    /// Creates an output folder with the specified path.
    /// </summary>
    /// <param name="path">The path of the output folder. If null, a default path will be used.</param>
    /// <returns>The path of the created output folder or null if operation failed.</returns>
    string? CreateOutputFolder(string? path);

    /// <summary>
    /// Checks if a directory exists at the specified path.
    /// </summary>
    /// <param name="path">The path of the directory to check.</param>
    /// <returns>True if the directory exists, otherwise false.</returns>
    bool DirectoryExists(string path);

    /// <summary>
    /// Gets the filename from the specified path.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <returns>The filename extracted from the path.</returns>
    string GetFilename(string path);

    /// <summary>
    /// Retrieves the lines of text from the specified text file.
    /// </summary>
    /// <param name="path">The path of the text file.</param>
    /// <returns>An enumerable collection of lines from the text file.</returns>
    IEnumerable<string>? GetLinesFromTextFile(string path);

    /// <summary>
    /// Returns the correct word file path based on the specified path and deck name.
    /// </summary>
    /// <param name="path">The path of the word file. If null, a default path will be used.</param>
    /// <param name="deckName">The name of the deck.</param>
    /// <returns>The correct word file path.</returns>
    string ReturnCorrectWordFilePath(string? path, string deckName = FilePaths.DEFAULT_WORD_FILE_NAME);
}

public class FileManager(ILogger<FileManager> logger) 
    : IFileManager
{
    private readonly ILogger<FileManager> _logger = logger;

    public async Task CreateImageFile(byte[] content, string folderPath, string fileName)
    {
        var path = Path.Combine(folderPath, fileName);
        try
        {
            await File.WriteAllBytesAsync(path, content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in file creation");
        }
    }

    public string? CreateOutputFolder(string? path)
    {
        path ??= Path.Combine(Directory.GetCurrentDirectory(), FilePaths.DEFAULT_FOLDER_NAME);

        if (!Directory.Exists(path))
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in creating output folder");
                return null;
            }
        }

        return Path.GetFullPath(path);
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public string GetFilename(string path) => Path.GetFileNameWithoutExtension(path);

    public IEnumerable<string>? GetLinesFromTextFile(string path)
    {
        var lines = new List<string>();

        try
        {
            using var reader = new StreamReader(path);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                lines.Add(line);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in reading file content");
            return null;
        }

        return lines;
    }

    public string ReturnCorrectWordFilePath(string? path, string deckName = FilePaths.DEFAULT_WORD_FILE_NAME)
    {
        if (string.IsNullOrEmpty(deckName))
        {
            deckName = FilePaths.DEFAULT_WORD_FILE_NAME;
        }

        if (Directory.Exists(path)) 
        {
            return Path.Combine(path, $"{deckName}.docx");
        }

        return Path.Combine(Directory.GetCurrentDirectory(), $"{deckName}.docx");
    }
}
