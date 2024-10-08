namespace Domain.Models.DTO.Archidekt;

public record DeckDTO(string? Name, ICollection<DeckCardDTO>? Cards);

public record DeckCardDTO(CardDTO? Card, int Quantity, string? Modifier = null);

public record CardDTO(OracleCardDTO? OracleCard, EditionDTO? Edition, string? CollectorNumber = null);

public record OracleCardDTO(string? Name = null, string? Layout = null);

public record EditionDTO(string? EditionCode = null);
