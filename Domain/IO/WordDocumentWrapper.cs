using DocumentFormat.OpenXml.Wordprocessing;
using OfficeIMO.Word;

namespace Domain.IO;

/// <summary>
/// Represents a wrapper for a Word document.
/// </summary>
public interface IWordDocumentWrapper
{
    /// <summary>
    /// Represents a Word document.
    /// </summary>
    WordDocument Create(string filePath);
    
    /// <summary>
    /// Sets the margins of the Word document.
    /// </summary>
    /// <param name="margin">The margin values to set.</param>
    void SetMargins(WordMargin margin);

    /// <summary>
    /// Sets the orientation of the Word document.
    /// </summary>
    /// <param name="orientation">The page orientation to set.</param>
    void SetOrientation(PageOrientationValues orientation);

    /// <summary>
    /// Sets the page size of the Word document.
    /// </summary>
    /// <param name="pageSize">The page size to set.</param>
    void SetPageSize(WordPageSize pageSize);

    /// <summary>
    /// Adds a new paragraph to the Word document.
    /// </summary>
    /// <returns>The newly added paragraph.</returns>
    WordParagraph AddParagraph();

    /// <summary>
    /// Adds an image to the specified paragraph in the Word document.
    /// </summary>
    /// <param name="paragraph">The paragraph to add the image to.</param>
    /// <param name="imageContent">The content of the image as a byte array.</param>
    /// <param name="fileName">The file name of the image.</param>
    /// <param name="width">The width of the image in points (optional).</param>
    /// <param name="height">The height of the image in points (optional).</param>
    void AddImage(WordParagraph paragraph, byte[] imageContent, string fileName, double? width, double? height);

    /// <summary>
    /// Saves the Word document.
    /// </summary>
    void Save();
}

public class WordDocumentWrapper : IWordDocumentWrapper, IDisposable
{
    private WordDocument? _wordDocument;

    public WordDocument Create(string filePath)
    {
        _wordDocument = WordDocument.Create(filePath);
        return _wordDocument;
    }

    public void SetMargins(WordMargin margin)
    {
        if (_wordDocument != null) _wordDocument.Margins.Type = margin;
        else throw new InvalidOperationException("Word document is not initialized");
    }

    public void SetOrientation(PageOrientationValues orientation)
    {
        if (_wordDocument != null) _wordDocument.PageSettings.Orientation = orientation;
        else throw new InvalidOperationException("Word document is not initialized");
    }

    public void SetPageSize(WordPageSize pageSize)
    {
        if (_wordDocument != null) _wordDocument.PageSettings.PageSize = pageSize;
        else throw new InvalidOperationException("Word document is not initialized");
    }

    public WordParagraph AddParagraph()
    {
        if (_wordDocument != null) return _wordDocument.AddParagraph();
        else throw new InvalidOperationException("Word document is not initialized");
    }

    public void AddImage(WordParagraph paragraph, byte[] imageContent, string fileName, double? width, double? height)
    {
        var stream = new MemoryStream(imageContent);
        paragraph.AddImage(stream, fileName, width, height);
    }

    public void Save()
    {
        _wordDocument?.Save();
    }

    public void Dispose()
    {
        _wordDocument?.Dispose();
    }
}