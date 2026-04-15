using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class User
{
    [BsonId]
    public ObjectId Id { get; set; }

    public string Name { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }

    public string Status { get; set; } = "active";
}