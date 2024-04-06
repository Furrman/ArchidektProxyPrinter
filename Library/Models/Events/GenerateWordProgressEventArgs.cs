namespace Library.Models.Events;

public class UpdateProgressEventArgs : EventArgs
{
    public double? Percent { get; set; }
    public string? ErrorMessage { get; set; }
}
