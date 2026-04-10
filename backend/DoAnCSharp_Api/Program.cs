using MongoDB.Driver;
using DoAnCSharp_Api.Models;
using MongoDB.Bson;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMobileApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var client = new MongoClient("mongodb://localhost:27017");
var database = client.GetDatabase("poi_db");
var poiCollection = database.GetCollection<Poi>("pois");

var app = builder.Build();

if (app.Environment.IsDevelopment()) { app.MapOpenApi(); }

app.UseCors("AllowMobileApp");

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600");
    }
});

// ==========================================
// CẤU HÌNH BASE URL CHO HÌNH ẢNH
// ==========================================
// Nếu chạy trên máy ảo Android: dùng "http://10.0.2.2:5188" (nhớ sửa port 5188 cho đúng với máy bạn)
// Nếu chạy trên điện thoại thật: dùng địa chỉ IP IPv4 của máy tính, ví dụ "http://192.168.1.15:5188"
string baseUrl = "http://10.0.2.2:5188"; 
// ==========================================

// 1. API lấy danh sách POI
app.MapGet("/api/poi", async (string lang = "vi") =>
{
    try
    {
        var pois = await poiCollection.Find(_ => true).ToListAsync();
        var result = pois.Select(p => new {
            p.Id,
            p.Name,
            p.Address,
            // Trả về URL Tuyệt đối thay vì đường dẫn tương đối
           Image = !string.IsNullOrWhiteSpace(p.Image) 
    ? $"{baseUrl}/images/{p.Image}?v={DateTime.Now.Ticks}" 
    : $"{baseUrl}/images/default.jpg?v={DateTime.Now.Ticks}",
            p.Lat,
            p.Lng,
            Description = p.Localizations?.FirstOrDefault(l => 
                l.Lang.Trim().ToLower() == lang.Trim().ToLower())?.Description ?? "No description"
        }).ToList();
        
        // Debug log
        Console.WriteLine($"[API /api/poi] Returned {result.Count} items with images");
        foreach (var item in result)
            Console.WriteLine($"  - {item.Name}: {item.Image}");
        
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] /api/poi: {ex.Message}");
        return Results.BadRequest(ex.Message);
    }
});

// 2. API lấy CHI TIẾT 1 POI
app.MapGet("/api/poi/{id}", async (string id, string lang) =>
{
    try {
        var poi = await poiCollection.Find(p => p.Id == id.Trim()).FirstOrDefaultAsync();
        
        if (poi == null) return Results.NotFound();

        var result = new {
            poi.Id,
            poi.Name,
            poi.Address,
            // Trả về URL Tuyệt đối
         Image = !string.IsNullOrWhiteSpace(poi.Image) 
    ? $"{baseUrl}/images/{poi.Image}?v={DateTime.Now.Ticks}" 
    : $"{baseUrl}/images/default.jpg?v={DateTime.Now.Ticks}",
            poi.Lat,
            poi.Lng,
            Description = poi.Localizations?.FirstOrDefault(l => 
                l.Lang.Trim().ToLower() == lang.Trim().ToLower())?.Description ?? "No description"
        };
        return Results.Ok(result);
    }
    catch { return Results.BadRequest("ID format error"); }
});

app.UseStaticFiles();
app.Run();