namespace ConsoleApp.Helpers;

// https://www.codeproject.com/Tips/5255878/A-Console-Progress-Bar-in-Csharp
internal static class ConsoleUtility
{
    private const char PROGRESS_BAR_BLOCK = '■';
    private const string PROGRESS_BAR_BACKSPACE = "\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b";
    private const string SPINNER_ANIMATION = "-\\|/";

    /// <summary>
    /// Checks if console cursor is at the beginning of the line
    /// </summary>
    /// <returns></returns>
    public static bool IsCursorAtZeroIndex()
    {
        return Console.CursorLeft == 0;
    }

    /// <summary>
    /// Method to write error messages in red color
    /// </summary>
    /// <param name="message"></param>
    public static void WriteErrorMessage(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        WriteInNewLine(message);
        Console.ResetColor();
    }

    /// <summary>
    /// Method to write warning messages in yellow color
    /// </summary>
    /// <param name="message"></param>
    public static void WriteWarningMessage(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    /// <summary>
    /// Write message always in new line
    /// </summary>
    /// <param name="message"></param>
    public static void WriteInNewLine(string message)
    {
        if (!IsCursorAtZeroIndex())
        {
            Console.WriteLine();
        }
        Console.WriteLine(message);
    }

    /// <summary>
    /// Method to write a progress bar
    /// </summary>
    /// <param name="percent"></param>
    /// <param name="update"></param>
    public static void WriteProgressBar(int percent, bool update = false)
    {
        // If update is true, move the cursor back to overwrite the previous progress
        if (update)
        {
            Console.Write(PROGRESS_BAR_BACKSPACE);
        }

        // Draw the progress bar
        Console.Write("[");
        var completedBlocks = (int)((percent / 10f) + .5f);
        for (var i = 0; i < 10; ++i)
        {
            if (i >= completedBlocks)
            {
                Console.Write(' ');
            }
            else
            {
                Console.Write(PROGRESS_BAR_BLOCK);
            }
        }
        Console.Write("] {0,3:##0}%", percent);    
    }

    /// <summary>
    /// Method to write a spinning animation for progress
    /// </summary>
    /// <param name="progress"></param>
    /// <param name="update"></param>
    public static void WriteSpinningProgress(int progress, bool update = false)
    {
        // If update is true, move the cursor back to overwrite the previous spinner
        if (update)
        {
            Console.Write("\b");
        }
        // Print the spinner animation character
        Console.Write(SPINNER_ANIMATION[progress % SPINNER_ANIMATION.Length]);
    }
}
