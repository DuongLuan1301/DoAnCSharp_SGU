using MongoDB.Driver;
using DoAnCSharp_Api.Models;

public static class MobileEndpoints
{
    public static void MapMobileEndpoints(this WebApplication app)
    {
        //GET ALL POIs API
        app.MapGet("/api/poi", async (
            HttpRequest request, // 🔥 THÊM DÒNG NÀY ĐỂ LẤY URL ĐỘNG
            string? lang, 
            IMongoCollection<Poi> poiCollection) =>
        {
            // 🔥 TỰ ĐỘNG LẤY BASE URL (Ví dụ: http://192.168.100.75:5188)
            var baseUrl = $"{request.Scheme}://{request.Host}";

            var pois = await poiCollection.Find(_ => true).ToListAsync();

            string safeLang = string.IsNullOrEmpty(lang) ? "vi" : lang.Trim().ToLower();

            var result = pois.Select(p => new
            {
                id = p.Id.ToString(), 
                p.Name,
                p.Address,
                // 🔥 THAY ĐỊA CHỈ CỨNG BẰNG BIẾN baseUrl
                image = string.IsNullOrWhiteSpace(p.Image) ? "" : 
                        p.Image.StartsWith("http") ? p.Image : $"{baseUrl}/images/{p.Image}",
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