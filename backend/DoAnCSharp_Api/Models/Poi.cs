using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace DoAnCSharp_Api.Models
{
    public class Poi
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("id")] 
        public string Id { get; set; } = string.Empty;

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("address")]
        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [BsonElement("image")]
        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;

        [BsonElement("lat")]
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [BsonElement("lng")]
        [JsonPropertyName("lng")]
        public double Lng { get; set; }

        [BsonElement("qrScans")]
        [JsonPropertyName("qrScans")]
        public int QrScans { get; set; } = 0;

        [BsonElement("audioListens")]
        [JsonPropertyName("audioListens")]
        public int AudioListens { get; set; } = 0;

        [BsonElement("views")]
        [JsonPropertyName("views")]
        public int Views { get; set; } = 0;

        [BsonElement("localizations")]
        [JsonPropertyName("localizations")]
        public List<Localization>? Localizations { get; set; }
    }

    public class Localization
    {
        [BsonElement("lang")]
        [JsonPropertyName("lang")]
        public string Lang { get; set; } = string.Empty;

        [BsonElement("description")]
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}