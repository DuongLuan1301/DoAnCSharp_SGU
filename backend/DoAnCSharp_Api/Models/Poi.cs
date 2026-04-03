using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DoAnCSharp_Api.Models;

[BsonIgnoreExtraElements] // CỰC KỲ QUAN TRỌNG: Để lờ đi cột _id của MongoDB
public class Poi
{
    // Đổi tên thành QrId để C# không tự ý ép nó về kiểu ObjectId (24 hex)
    // Nhưng dùng [BsonElement("Id")] để nó vẫn lấy dữ liệu từ cột "Id" bạn đã nhập trong Compass
    [BsonElement("Id")]
    public string QrId { get; set; } = null!;

    [BsonElement("Name")]
    public string Name { get; set; } = null!;

    [BsonElement("Address")]
    public string Address { get; set; } = null!;

    [BsonElement("Lat")]
    public double Lat { get; set; }

    [BsonElement("Lng")]
    public double Lng { get; set; }

    [BsonElement("Localizations")]
    public List<Localization>? Localizations { get; set; }
}

public class Localization
{
    [BsonElement("Lang")]
    public string? Lang { get; set; }

    [BsonElement("Description")]
    public string? Description { get; set; }
}