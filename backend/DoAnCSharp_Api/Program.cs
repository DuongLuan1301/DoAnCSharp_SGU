using MongoDB.Driver;
using DoAnCSharp_Api.Models;
using MongoDB.Bson;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

// 1. Configure CORS (Chỉ cần khai báo 1 lần)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 2. Kết nối MongoDB
var client = new MongoClient("mongodb://localhost:27017");
var database = client.GetDatabase("poi_db");
var poiCollection = database.GetCollection<Poi>("pois");

// 3. Build App
var app = builder.Build();

if (app.Environment.IsDevelopment()) 
{ 
    app.MapOpenApi(); 
}

// 4. Đăng ký Middleware
app.UseCors("AllowAll");
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600");
    }
});
app.MapMobileEndpoints(poiCollection);
app.MapPoiEndpoints(poiCollection);
app.MapUploadEndpoints();

// ==========================================
// CẤU HÌNH BASE URL CHO APP MAUI
// ==========================================
string baseUrl = "http://10.0.2.2:5188"; 
// ==========================================


// =========================================================================
// REGION 1: API CHO APP MAUI & PARTNER PORTAL
// =========================================================================
#region API CHO APP MAUI

app.MapGet("/api/poi/{id}", async (string id, string lang = "vi") =>
{
    try {
        var p = await poiCollection.Find(poi => poi.Id == id.Trim()).FirstOrDefaultAsync();
        if (p == null) return Results.NotFound();

        string desc = "No description";
        if (p.Localizations != null && p.Localizations.Any())
        {
            var loc = p.Localizations.FirstOrDefault(l => l.Lang != null && l.Lang.Trim().ToLower() == lang.Trim().ToLower());
            if (loc != null && !string.IsNullOrWhiteSpace(loc.Description)) desc = loc.Description;
        }

        string imgUrl = $"{baseUrl}/images/default.jpg?v={DateTime.Now.Ticks}";
        if (!string.IsNullOrWhiteSpace(p.Image))
        {
            if (p.Image.StartsWith("http")) imgUrl = p.Image;
            else imgUrl = $"{baseUrl}/images/{p.Image}?v={DateTime.Now.Ticks}";
        }

        var result = new { p.Id, p.Name, p.Address, Image = imgUrl, p.Lat, p.Lng, Description = desc };
        return Results.Ok(result);
    }
    catch { return Results.BadRequest("ID format error"); }
});


app.MapPut("/api/poi/{id}", async (string id, Poi updatedPoi) =>
{
    try
    {
        updatedPoi.Id = id; 
        if (!string.IsNullOrWhiteSpace(updatedPoi.Image))
        {
            string pathWithoutQuery = updatedPoi.Image.Split('?')[0]; 
            updatedPoi.Image = System.IO.Path.GetFileName(pathWithoutQuery); 
        }
        var result = await poiCollection.ReplaceOneAsync(p => p.Id == id, updatedPoi);
        if (result.MatchedCount == 0) return Results.NotFound("Không tìm thấy POI");
        return Results.Ok(new { message = "Cập nhật thành công" });
    }
    catch (Exception ex) { return Results.BadRequest(ex.Message); }
});

#endregion


// =========================================================================
// REGION 2: API CHO WEB ADMIN (/admin/poi & /upload-image)
// =========================================================================
#region API CHO WEB ADMIN

// 3. API DELETE: Xóa POI
app.MapDelete("/admin/poi/{id}", async (string id) =>
{
    try 
    {
        // Thêm .Trim() để dọn dẹp khoảng trắng dư thừa ở ID
        var result = await poiCollection.DeleteOneAsync(p => p.Id == id.Trim());
        
        if (result.DeletedCount > 0) 
            return Results.Ok(new { message = "Xóa thành công" });
            
        return Results.NotFound(new { message = "Không tìm thấy POI trong Database" });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = "Lỗi định dạng ID: " + ex.Message });
    }
});

// 4. Upload Hình Ảnh
app.MapPost("/upload-image", async (IFormFile file) =>
{
    try
    {
        if (file == null || file.Length == 0) 
            return Results.BadRequest(new { message = "Chưa chọn file" });

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        
        if (!Directory.Exists(uploadsFolder)) 
            Directory.CreateDirectory(uploadsFolder);

        var fileName = file.FileName.Replace(" ", "-"); 
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Results.Ok(new { fileName = fileName });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
}).DisableAntiforgery(); 

#endregion

// 5. Chạy Server (Lệnh cuối cùng của ứng dụng)
app.Run("http://0.0.0.0:5188");