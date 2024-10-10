using System.Text.Json.Serialization;

namespace Domain.Models.DTO.Scryfall;

public record CardSearchDTO(ICollection<CardDataDTO?>? Data);

public record CardDataDTO
{
    public string? Name { get; set; }
    public string? Lang { get; set; }
    public string? Set { get; set; }
    [JsonPropertyName("tcgplayer_etched_id")]
    public int? TcgPlayerEtchedId { get; set; }
    [JsonPropertyName("all_parts")]
    public ICollection<CardPartDTO>? AllParts { get; set; }
    [JsonPropertyName("card_faces")]
    public ICollection<CardFaceDTO>? CardFaces { get; set; }
    [JsonPropertyName("image_uris")]
    public CardImageUriDTO? ImageUriData { get; set; }
}

public record CardFaceDTO(string? Name, [property: JsonPropertyName("image_uris")] CardImageUriDTO? ImageUriData);

public record CardImageUriDTO(string? Large);

public record CardPartDTO(string Name, string Component, string Uri);
