namespace Domain.Models.Events;

public class GetDeckDetailsProgressEventArgs : EventArgs
{
    public double? Percent { get; set; }
    public string? ErrorMessage { get; set; }
}
