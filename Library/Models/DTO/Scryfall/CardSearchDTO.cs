using System.Text.Json.Serialization;

namespace Library.Models.DTO.Scryfall;

public record CardSearchDTO(ICollection<CardDataDTO?>? Data);

public class CardDataDTO
{
    public string? Name { get; set; }
    public string? Lang { get; set; }
    public string? Set { get; set; }
    [JsonPropertyName("tcgplayer_etched_id")]
    public int? TcgplayerEtchedId { get; set; }
    [JsonPropertyName("all_parts")]
    public ICollection<CardPartDTO>? AllParts { get; set; }
    [JsonPropertyName("card_faces")]
    public ICollection<CardFaceDTO>? CardFaces { get; set; }
    [JsonPropertyName("image_uris")]
    public CardImageUriDTO? ImageUris { get; set; }
}

public record CardFaceDTO(string? Name, [property: JsonPropertyName("image_uris")] CardImageUriDTO? ImageUris);

public record CardImageUriDTO(string? Large);

public record CardPartDTO(string Name, string Component, string Uri);
