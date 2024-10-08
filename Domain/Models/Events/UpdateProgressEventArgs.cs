namespace Domain.Models.Events;

public class UpdateProgressEventArgs : EventArgs
{
    public CreateMagicDeckDocumentStageEnum Stage { get; set; }
    public double? Percent { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum CreateMagicDeckDocumentStageEnum
{
    GetDeckDetails,
    SaveToDocument
}