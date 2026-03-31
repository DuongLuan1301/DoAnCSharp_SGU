using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

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

        [BsonElement("lat")]
        public double Lat { get; set; }

        [BsonElement("lng")]
        public double Lng { get; set; }

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

