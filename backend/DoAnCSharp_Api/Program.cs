using MongoDB.Driver;
using DoAnCSharp_Api.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var client = new MongoClient("mongodb://localhost:27017");
var database = client.GetDatabase("poi_db");
var poiCollection = database.GetCollection<Poi>("pois");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 1. API LẤY DANH SÁCH
// SỬA: Thêm string? và gán mặc định = "vi" để không bị lỗi khi thiếu tham số
app.MapGet("/api/poi", async (string? lang = "vi") =>
{
    var pois = await poiCollection.Find(_ => true).ToListAsync();
    var result = pois.Select(p => new
    {
        Id = p.QrId,
        p.Name,
        p.Address,
        p.Lat,
        p.Lng,
        Description = p.Localizations?
            .FirstOrDefault(l => l.Lang == lang)?.Description
            ?? "No description"
    });

    return Results.Ok(result);
});

// 🔥 2. API LẤY CHI TIẾT THEO ID
// SỬA: Tương tự, thêm string? và gán mặc định = "vi"
app.MapGet("/api/poi/{id}", async (string id, string? lang = "vi") =>
{
    var poi = await poiCollection.Find(p => p.QrId == id).FirstOrDefaultAsync();

    if (poi == null)
    {
        return Results.NotFound(new { message = $"Không tìm thấy địa điểm: {id}" });
    }

    var result = new
    {
        Id = poi.QrId,
        poi.Name,
        poi.Address,
        poi.Lat,
        poi.Lng,
        Description = poi.Localizations?
            .FirstOrDefault(l => l.Lang == lang)?.Description
            ?? "No description"
    };

    return Results.Ok(result);
});

app.Run();