namespace Library.Models.DTO.Scryfall;

public class CardSearchDTO
{
    public ICollection<CardDataDTO?>? Data { get; set; }
}

public class CardDataDTO
{
    public string? Name { get; set; }
    public ICollection<CardFaceDTO>? Card_faces { get; set; }
    public CardImageUriDTO? Image_uris { get; set; }
}

public class CardFaceDTO
{
    public string? Name { get; set; }
    public CardImageUriDTO? Image_uris { get; set; }
}

public class CardImageUriDTO
{
    public string? Large { get; set; }
}