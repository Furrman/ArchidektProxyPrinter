using Microsoft.Extensions.Logging;

namespace Library.IO;

public class FileManager(ILogger<FileManager> logger)
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

    public string CreateOutputFolder(string? path)
    {
        path ??= Path.Combine(Directory.GetCurrentDirectory(), Constants.DEFAULT_FOLDER_NAME);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return Path.GetFullPath(path);
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public string GetFilename(string path) => Path.GetFileNameWithoutExtension(path);

    public string ReturnCorrectWordFilePath(string? path, string? deckName = null)
    {
        if (Directory.Exists(path)) 
        {
            return Path.Combine(path, $"{deckName}.docx");
        }

        return Path.Combine(Directory.GetCurrentDirectory(),
            Constants.DEFAULT_FOLDER_NAME,
            deckName != null ? $"{deckName}.docx" : Constants.DEFAULT_WORD_FILE_NAME);
    }
}
