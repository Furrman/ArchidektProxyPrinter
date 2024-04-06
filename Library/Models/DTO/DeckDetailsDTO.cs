namespace Library.Models.DTO;

public class DeckDetailsDTO 
{
    public string Name { get; set; } = string.Empty;
    public HashSet<CardEntryDTO> Cards { get; set; } = [];
}
