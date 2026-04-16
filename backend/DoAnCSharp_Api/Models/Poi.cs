using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace DoAnCSharp_Api.Models
{
    [BsonIgnoreExtraElements] // Giúp tránh lỗi FormatException khi MongoDB có data thừa
    public class Poi
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        // 🔥 THÊM DÒNG NÀY: Dùng để xác định POI này thuộc về Client nào
        [BsonElement("clientId")]
        [JsonPropertyName("clientId")]
        public string ClientId { get; set; } = string.Empty;

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

        // 1. Lượt quét QR
        [BsonElement("qrScans")]
        [JsonPropertyName("qrScans")] 
        public int QrScans { get; set; } = 0;

        // 2. Lượt nghe Audio
        [BsonElement("audioListens")]
        [JsonPropertyName("audioListens")] 
        public int AudioListens { get; set; } = 0;

        // 3. Tổng lượt truy cập
        [BsonElement("views")]
        [JsonPropertyName("views")] 
        public int Views { get; set; } = 0;

        [BsonElement("localizations")]
        [JsonPropertyName("localizations")]
        public List<Localization>? Localizations { get; set; } // nullable
    }

    [BsonIgnoreExtraElements]
    public class Localization
    {
        [BsonElement("lang")]
        [JsonPropertyName("lang")]
        public string? Lang { get; set; }  // nullable

        [BsonElement("description")]
        [JsonPropertyName("description")]
        public string? Description { get; set; } // nullable
    }
}