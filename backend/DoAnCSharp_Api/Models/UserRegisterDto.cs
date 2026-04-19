using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
public class UserRegisterDto
{
    public string Name { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}