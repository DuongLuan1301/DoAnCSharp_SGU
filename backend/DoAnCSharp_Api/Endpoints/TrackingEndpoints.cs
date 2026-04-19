using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using DoAnCSharp_Api.Models;
using System;

namespace DoAnCSharp_Api.Endpoints
{
    public static class TrackingEndpoints
    {
        public static void MapTrackingEndpoints(this WebApplication app)
        {
            // Ghi nhận lượt truy cập chung (View)
            app.MapPost("/api/poi/{id}/view", async (string id, IMongoCollection<Poi> poiCollection) =>
            {
                var filter = Builders<Poi>.Filter.Eq(p => p.Id, id.Trim());
                // Dùng $inc để cộng 1 vào cột Views
                var update = Builders<Poi>.Update.Inc(p => p.Views, 1);

                await poiCollection.UpdateOneAsync(filter, update);
                return Results.Ok(new { message = "View recorded" });
            });

            // Ghi nhận lượt quét QR
            app.MapPost("/api/poi/{id}/scan-qr", async (string id, IMongoCollection<Poi> poiCollection) =>
            {
                var filter = Builders<Poi>.Filter.Eq(p => p.Id, id.Trim());
                // Cộng 1 cho QR
                var update = Builders<Poi>.Update
                                .Inc(p => p.QrScans, 1);

                await poiCollection.UpdateOneAsync(filter, update);
                return Results.Ok(new { message = "QR scan recorded" });
            });

            // Ghi nhận lượt nghe Audio
            app.MapPost("/api/poi/{id}/listen-audio", async (string id, IMongoCollection<Poi> poiCollection) =>
            {
                var filter = Builders<Poi>.Filter.Eq(p => p.Id, id.Trim());
                // Chỉ cộng Audio, không cộng View vì người dùng đã vào trang chi tiết trước đó rồi
                var update = Builders<Poi>.Update.Inc(p => p.AudioListens, 1);

                await poiCollection.UpdateOneAsync(filter, update);
                return Results.Ok(new { message = "Audio listen recorded" });
            });
        }
    }
}