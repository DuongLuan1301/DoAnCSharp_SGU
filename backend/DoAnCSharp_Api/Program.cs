using MongoDB.Driver;
using DoAnCSharp_Api.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

// Cấu hình MongoDB
var client = new MongoClient("mongodb://localhost:27017");
var database = client.GetDatabase("poi_db");
var poiCollection = database.GetCollection<Poi>("pois");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

<<<<<<< Updated upstream
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
=======
// TẠM TẮT dòng này để Android/Emulator kết nối qua HTTP (IP nội bộ) dễ hơn
// app.UseHttpsRedirection();

// ---------------------------------------------------------
// 1. API LẤY DANH SÁCH (Cho trang Places trên Mobile)
// URL: http://localhost:5188/api/poi
// ---------------------------------------------------------
app.MapGet("/api/poi", async () =>
{
    var allPois = await poiCollection.Find(_ => true).ToListAsync();
    return Results.Ok(allPois);
});

// ---------------------------------------------------------
// 2. API LẤY CHI TIẾT THEO QR_ID (Cho trang QR)
// URL: http://localhost:5188/api/poi/BUNBO
// ---------------------------------------------------------
app.MapGet("/api/poi/{id}", async (string id, string? lang = "vi") =>
{
    // Tìm theo qr_id ngắn (ví dụ: BUNBO, PHO) đã thêm vào class Poi
    var poi = await poiCollection.Find(p => p.qr_id == id).FirstOrDefaultAsync();

    if (poi == null)
    {
        return Results.NotFound(new { message = $"Không tìm thấy địa điểm: {id}" });
    }

    // Trả về dữ liệu với tên biến viết thường để khớp với Model bên Mobile
    var result = new 
    {
        id = poi.qr_id,
        name = poi.Name,
        address = poi.Address,
        lat = poi.Lat,
        lng = poi.Lng,
        description = poi.Localizations?
            .FirstOrDefault(l => l.Lang == lang)?.Description 
>>>>>>> Stashed changes
            ?? "No description"
    };

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