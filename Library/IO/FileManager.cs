﻿namespace Library.IO;

public class FileManager
{
    public string CreateOutputFolder(string? path)
    {
        if (path == null)
        {
            path = Path.Combine(Directory.GetCurrentDirectory(), Constants.DEFAULT_FOLDER_NAME);
        }

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

    public string ReturnCorrectFilePath(string? path, string? deckName = null)
    {
        if (Path.Exists(path)) 
        {
            return path;
        }

        return Path.Combine(Directory.GetCurrentDirectory(),
            Constants.DEFAULT_FOLDER_NAME,
            deckName != null ? $"{deckName}.docx" : Constants.DEFAULT_WORD_FILE_NAME);
    }
}
