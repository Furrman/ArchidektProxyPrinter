namespace Library.Models.DTO;

public class DeckDetailsDTO 
{
    public string Name { get; set; } = string.Empty;
    public List<CardEntryDTO> Cards { get; set; } = [];
}
