namespace Library.Models.DTO.Archidekt;

public class DeckDTO
{
    public ICollection<DeckCardDTO>? Cards { get; set; } = null;
    public string Name { get; set; }
}

public class DeckCardDTO
{
    public CardDTO? Card { get; set; }
    public int Quantity { get; set; }
}

public class CardDTO
{
    public OracleCardDTO? OracleCard { get; set; }
}

public class OracleCardDTO
{
    public string? Name { get; set; }
}