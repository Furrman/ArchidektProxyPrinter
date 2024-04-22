namespace Library.Models.DTO.Archidekt;

public class DeckDTO
{
    public ICollection<DeckCardDTO>? Cards { get; set; }
    public string? Name { get; set; }
}

public class DeckCardDTO
{
    public CardDTO? Card { get; set; }
    public int Quantity { get; set; }
    public string? Modifier { get; set; }
}

public class CardDTO
{
    public OracleCardDTO? OracleCard { get; set; }
    public EditionDTO? Edition { get; set; }
    public string? CollectorNumber { get; set; }
}

public class OracleCardDTO
{
    public string? Name { get; set; }
    public string? Layout { get; set; }
}

public class EditionDTO
{
    public string? EditionCode { get; set; }
}