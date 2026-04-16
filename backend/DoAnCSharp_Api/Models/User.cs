using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace DoAnCSharp_Api.Models // Đảm bảo đúng namespace của project bạn
{
    [BsonIgnoreExtraElements] // Thêm dòng này để tránh lỗi nếu DB có cột thừa
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)] // Báo MongoDB tự convert String <-> ObjectId
        [JsonPropertyName("id")] // Trả về frontend với tên viết thường cho đồng bộ
        public string Id { get; set; } = string.Empty;

        [BsonElement("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("phone")]
        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [BsonElement("email")]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("password")]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [BsonElement("status")]
        [JsonPropertyName("status")]
        public string Status { get; set; } = "active";
    }
}