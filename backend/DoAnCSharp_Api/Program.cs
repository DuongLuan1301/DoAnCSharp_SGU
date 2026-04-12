using MongoDB.Driver;
using DoAnCSharp_Api.Models;
using MongoDB.Bson;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var client = new MongoClient("mongodb://localhost:27017");
var database = client.GetDatabase("poi_db");
var poiCollection = database.GetCollection<Poi>("pois");


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        p => p.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();
app.UseCors("AllowAll");
app.UseStaticFiles();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
// app.UseHttpsRedirection();

// gọi endpoints
app.MapMobileEndpoints(poiCollection);
app.MapPoiEndpoints(poiCollection);
app.MapUploadEndpoints();

app.Run();

if (app.Environment.IsDevelopment()) { app.MapOpenApi(); }

app.UseCors("AllowAll");

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600");
    }
});

// ==========================================
// CẤU HÌNH BASE URL CHO APP MAUI
// ==========================================
string baseUrl = "http://10.0.2.2:5188"; 
// ==========================================

#region API CHO APP MAUI & PARTNER PORTAL (Giữ nguyên không thay đổi)

app.MapGet("/api/poi", async (string lang = "vi") =>
{
    try
    {
        var pois = await poiCollection.Find(_ => true).ToListAsync();
        var result = pois.Select(p => 
        {
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

            return new { p.Id, p.Name, p.Address, Image = imgUrl, p.Lat, p.Lng, Description = desc };
        }).ToList();
        
        return Results.Ok(result);
    }
    catch (Exception ex) { return Results.BadRequest(ex.Message); }
});

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

app.MapPost("/api/poi", async (Poi newPoi) =>
{
    try
    {
        if (!string.IsNullOrWhiteSpace(newPoi.Image))
        {
            string pathWithoutQuery = newPoi.Image.Split('?')[0]; 
            newPoi.Image = System.IO.Path.GetFileName(pathWithoutQuery); 
        }
        await poiCollection.InsertOneAsync(newPoi);
        return Results.Ok(new { message = "Thêm thành công", id = newPoi.Id });
    }
    catch (Exception ex) { return Results.BadRequest(ex.Message); }
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
// THÊM MỚI: API DÀNH RIÊNG CHO WEB ADMIN CỦA BẠN
// =========================================================================

#region API CHO WEB ADMIN (/admin/poi & /upload-image)

// 1. API GET: Lấy toàn bộ POI gốc (dành cho index.js)
app.MapGet("/admin/poi", async () =>
{
    var pois = await poiCollection.Find(_ => true).ToListAsync();
    // Trả về Raw Data, không chế biến Image URL để Web Admin tự nối chuỗi
    return Results.Ok(pois);
});

// 2. API POST: Thêm POI mới (dành cho add.html)
app.MapPost("/admin/poi", async (Poi newPoi) =>
{
    try
    {
        await poiCollection.InsertOneAsync(newPoi);
        return Results.Ok(newPoi);
    }
    catch (Exception ex) { return Results.BadRequest(ex.Message); }
});

// 3. API DELETE: Xóa POI (dành cho nút Delete trong index.js)
app.MapDelete("/admin/poi/{id}", async (string id) =>
{
    var result = await poiCollection.DeleteOneAsync(p => p.Id == id);
    if (result.DeletedCount > 0) return Results.Ok(new { message = "Xóa thành công" });
    return Results.NotFound(new { message = "Không tìm thấy POI" });
});

// 4. API UPLOAD IMAGE: Hứng file ảnh thực tế tải lên từ Web Admin
app.MapPost("/upload-image", async (IFormFile file) =>
{
    try
    {
        if (file == null || file.Length == 0) 
            return Results.BadRequest(new { message = "Chưa chọn file" });

        // Tạo đường dẫn đến thư mục wwwroot/images
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
        
        // Nếu thư mục chưa tồn tại thì tự động tạo
        if (!Directory.Exists(uploadsFolder)) 
            Directory.CreateDirectory(uploadsFolder);

        // Lưu file bằng tên gốc của nó (hoặc bạn có thể dùng Guid để không trùng lặp)
        // Thay khoảng trắng bằng dấu gạch ngang cho an toàn
        var fileName = file.FileName.Replace(" ", "-"); 
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Trả về đúng object json mà add.html đang mong đợi: uploadData.fileName
        return Results.Ok(new { fileName = fileName });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
}).DisableAntiforgery(); // Tắt check Antiforgery để cho phép upload file từ client ngoài

#endregion

app.UseStaticFiles();
app.Run("http://0.0.0.0:5188");
