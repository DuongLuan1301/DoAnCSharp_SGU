using MongoDB.Driver;
using DoAnCSharp_Api.Models;

namespace DoAnCSharp_Api.Endpoints // Thêm namespace cho chuẩn nếu cần
{
    //These APIs are for mobile app
    public static class MobileEndpoints
    {
        public static void MapMobileEndpoints(this WebApplication app)
        {
            //GET ALL POIs API
            app.MapGet("/api/poi", async (
                string lang,
                IMongoCollection<Poi> poiCollection) =>
            {
                var pois = await poiCollection.Find(_ => true).ToListAsync();

                var result = pois.Select(p => new
                {
                    Id = p.Id, // 🔥 ĐÂY LÀ DÒNG QUAN TRỌNG NHẤT BỊ THIẾU
                    p.Name,
                    p.Address,
                    image = $"http://10.0.2.2:5188/images/{p.Image}",
                    p.Lat,
                    p.Lng,
                    Description = p.Localizations?
                        .FirstOrDefault(l => l.Lang == lang)?.Description
                        ?? "No description"
                });

                return Results.Ok(result);
            });
        }
    }
}