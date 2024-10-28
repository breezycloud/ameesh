using System.Text.Json.Serialization;

namespace Shared.Models.Locations;

public class LgaModel
{    
    [JsonPropertyName("lga")]
    public string? Lga { get; set; }
    [JsonPropertyName("wards")]
    public List<string>? Wards { get; set; }
}