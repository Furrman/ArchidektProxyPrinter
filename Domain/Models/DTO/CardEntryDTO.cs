namespace Domain.Models.DTO;

public class CardEntryDTO : IEquatable<CardEntryDTO>
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    /// <summary>
    /// Not supported by Archidekt file export
    /// </summary>
    public string? ExpansionCode { get; set; }
    /// <summary>
    /// Not supported by Archidekt file export
    /// </summary>
    public string? CollectorNumber { get; set; }
    /// <summary>
    /// Not supported by Archidekt file export
    /// </summary>
    public bool Art { get; set; }
    public bool Etched { get; set; }
    public bool Foil { get; set; }
    public HashSet<CardSideDTO> CardSides { get; set; } = [];
    public List<CardTokenDTO> Tokens { get; set; } = [];


    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Quantity, CardSides);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        return Equals(obj as CardEntryDTO);
    }
    
    public bool Equals(CardEntryDTO? other)
    {
        if (other is null)
            return false;

        return Name == other.Name 
            && Quantity == other.Quantity 
            && CardSides == CardSides;
    }
}
