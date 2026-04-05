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

<<<<<<< Updated upstream
    [BsonElement("Name")]
    public string Name { get; set; } = null!;
=======
        // 1. THÊM TRƯỜNG NÀY ĐỂ KHỚP VỚI DATABASE VÀ FIX LỖI BUILD
        [BsonElement("qr_id")]
        public string? qr_id { get; set; } 

        [BsonElement("name")]
        public string Name { get; set; }
>>>>>>> Stashed changes

    [BsonElement("Address")]
    public string Address { get; set; } = null!;

    [BsonElement("Lat")]
    public double Lat { get; set; }

    [BsonElement("Lng")]
    public double Lng { get; set; }

<<<<<<< Updated upstream
    [BsonElement("Localizations")]
    public List<Localization>? Localizations { get; set; }
}

public class Localization
{
    [BsonElement("Lang")]
    public string? Lang { get; set; }

    [BsonElement("Description")]
    public string? Description { get; set; }
=======
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
>>>>>>> Stashed changes
}