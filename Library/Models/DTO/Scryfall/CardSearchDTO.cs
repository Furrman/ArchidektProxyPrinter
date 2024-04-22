using System.Text.Json.Serialization;

namespace Library.Models.DTO.Scryfall;

public class CardSearchDTO
{
    public ICollection<CardDataDTO?>? Data { get; set; }
}

public class CardDataDTO
{
    public string? Name { get; set; }
    public string? Set { get; set; }
    [JsonPropertyName("tcgplayer_etched_id")]
    public int? TcgplayerEtchedId { get; set; }

    [JsonPropertyName("card_faces")]
    public ICollection<CardFaceDTO>? CardFaces { get; set; }
    [JsonPropertyName("image_uris")]
    public CardImageUriDTO? ImageUris { get; set; }
}

public class CardFaceDTO
{
    public string? Name { get; set; }
    [JsonPropertyName("image_uris")]
    public CardImageUriDTO? ImageUris { get; set; }
}

public class CardImageUriDTO
{
    public string? Large { get; set; }
}