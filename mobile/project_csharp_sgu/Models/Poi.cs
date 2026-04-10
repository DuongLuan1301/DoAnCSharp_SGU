using System.Text.Json.Serialization;

#nullable enable

namespace project_csharp_sgu.Models;

public class Poi
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lng")]
    public double Lng { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    // Trường này tính toán tại Mobile nên không cần JsonPropertyName
    public string? Distance { get; set; }
}