using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace DoAnCSharp_Api.Models
{
    public class Poi
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("address")]
        public string Address { get; set; }

        [BsonElement("image")]
        public string Image { get; set; }

        [BsonElement("lat")]
        public double Lat { get; set; }

        [BsonElement("lng")]
        public double Lng { get; set; }
// 1. Lượt quét QR
        [BsonElement("qrScans")]
        [JsonPropertyName("qrScans")] // Thêm dòng này
        public int QrScans { get; set; } = 0;

        // 2. Lượt nghe Audio
        [BsonElement("audioListens")]
        [JsonPropertyName("audioListens")] // Thêm dòng này
        public int AudioListens { get; set; } = 0;

        // 3. Tổng lượt truy cập
        [BsonElement("views")]
        [JsonPropertyName("views")] // Thêm dòng này
        public int Views { get; set; } = 0;

        [BsonElement("localizations")]
        public List<Localization>? Localizations { get; set; } // nullable
    }

    public class Localization
    {
        [BsonElement("lang")]
        public string? Lang { get; set; }  // nullable

        [BsonElement("description")]
        public string? Description { get; set; } // nullable
    }
}