namespace Library.Models.Events;

public class DownloadDeckProgressEventArgs : EventArgs
{
    public double? Percent { get; set; }
    public string? ErrorMessage { get; set; }
}
