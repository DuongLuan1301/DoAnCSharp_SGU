using MongoDB.Driver;
using DoAnCSharp_Api.Models;

//These APIs are for mobile app
public static class MobileEndpoints
{
    public static void MapMobileEndpoints(this WebApplication app)
    {
        //GET ALL POIs API
        app.MapGet("/api/poi", async (
            string? lang, // 🔥 FIX 1: Thêm dấu ? để cho phép null (tránh lỗi 400 Bad Request)
            IMongoCollection<Poi> poiCollection) =>
        {
            var pois = await poiCollection.Find(_ => true).ToListAsync();

            // Đảm bảo lang luôn có giá trị (mặc định là tiếng Việt nếu App gửi lên bị rỗng)
            string safeLang = string.IsNullOrEmpty(lang) ? "vi" : lang.Trim().ToLower();

            var result = pois.Select(p => new
            {
                id = p.Id.ToString(), // 🔥 FIX 2: Bắt buộc phải trả về ID cho App Mobile
                p.Name,
                p.Address,
                // Chống lỗi nếu Database bị mất ảnh
                image = string.IsNullOrWhiteSpace(p.Image) ? "" : 
                        p.Image.StartsWith("http") ? p.Image : $"http://192.168.31.34:5188/images/{p.Image}",
                p.Lat,
                p.Lng,
                Description = p.Localizations?
                    .FirstOrDefault(l => l.Lang != null && l.Lang.Trim().ToLower() == safeLang)?.Description
                    ?? "No description"
            });

            return Results.Ok(result);
        });
    }
}