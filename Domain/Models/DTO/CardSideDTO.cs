namespace Domain.Models.DTO;

public class CardSideDTO : IEquatable<CardSideDTO>
{
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;


    public override int GetHashCode()
    {
        return HashCode.Combine(Name, ImageUrl);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        return Equals(obj as CardSideDTO);
    }
    
    public bool Equals(CardSideDTO? other)
    {
        if (other is null)
            return false;

        return Name == other.Name && ImageUrl == other.ImageUrl;
    }
}
