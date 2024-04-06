namespace Library.Models.DTO;

public class DeckDetailsDTO
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, CardEntryDTO> Cards { get; set; } = [];
}
