using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using DoAnCSharp_Api.Models;

namespace DoAnCSharp_Api.Endpoints
{
    public static class TrackingEndpoints
    {
        public static void MapTrackingEndpoints(this WebApplication app, IMongoCollection<Poi> poiCollection)
        {
            // 1. Ghi nhận lượt truy cập chung (View)
            app.MapPost("/api/poi/{id}/view", async (string id) =>
            {
                try
                {
                    var filter = Builders<Poi>.Filter.Eq(p => p.Id, id.Trim());
                    // Dùng $inc để cộng 1 vào cột Views
                    var update = Builders<Poi>.Update.Inc(p => p.Views, 1);
                    
                    await poiCollection.UpdateOneAsync(filter, update);
                    return Results.Ok(new { message = "View recorded" });
                }
                catch (Exception ex) { return Results.BadRequest(ex.Message); }
            });

            // 2. Ghi nhận lượt quét QR
            app.MapPost("/api/poi/{id}/scan-qr", async (string id) =>
            {
                try
                {
                    var filter = Builders<Poi>.Filter.Eq(p => p.Id, id.Trim());
                    // Cộng 1 cho QR, đồng thời cộng 1 cho Tổng View
                    var update = Builders<Poi>.Update
                                    .Inc(p => p.QrScans, 1)
                                    .Inc(p => p.Views, 1); 
                    
                    await poiCollection.UpdateOneAsync(filter, update);
                    return Results.Ok(new { message = "QR scan recorded" });
                }
                catch (Exception ex) { return Results.BadRequest(ex.Message); }
            });

            // 3. Ghi nhận lượt nghe Audio
            app.MapPost("/api/poi/{id}/listen-audio", async (string id) =>
            {
                try
                {
                    var filter = Builders<Poi>.Filter.Eq(p => p.Id, id.Trim());
                    // Chỉ cộng Audio, không cộng View vì người dùng đã vào trang chi tiết trước đó rồi
                    var update = Builders<Poi>.Update.Inc(p => p.AudioListens, 1);
                    
                    await poiCollection.UpdateOneAsync(filter, update);
                    return Results.Ok(new { message = "Audio listen recorded" });
                }
                catch (Exception ex) { return Results.BadRequest(ex.Message); }
            });
        }
    }
}