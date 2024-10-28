using System.Text.Json.Serialization;

namespace Shared.Models.Locations;

public class StateLgaWard
{
    [JsonPropertyName("state")]
    public string? State { get; set; }
    [JsonPropertyName("lgas")]
    public List<LgaModel>? Lgas { get; set; }
    
}