namespace Library.Models.DTO.Archidekt;

public record DeckDTO(ICollection<DeckCardDTO>? Cards, string? Name);

public record DeckCardDTO(CardDTO? Card, int Quantity, string? Modifier);

public record CardDTO(OracleCardDTO? OracleCard, EditionDTO? Edition, string? CollectorNumber);

public record OracleCardDTO(string? Name, string? Layout);

public record EditionDTO(string? EditionCode);
