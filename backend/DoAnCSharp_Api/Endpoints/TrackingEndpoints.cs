using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using DoAnCSharp_Api.Models;
using System;
using System.Collections.Concurrent; // 🔥 Cần thiết cho Dictionary đa luồng
using System.Linq; // 🔥 Cần thiết cho hàm Count() và Where()

namespace DoAnCSharp_Api.Endpoints
{
    public static class TrackingEndpoints
    {
        // 🔥 BIẾN LƯU TRỮ NGƯỜI DÙNG ONLINE (Key: DeviceId, Value: Lần ping cuối)
        private static ConcurrentDictionary<string, DateTime> _onlineUsers = new();

        public static void MapTrackingEndpoints(this WebApplication app)
        {
            // ==========================================
            // 1. CÁC TÍNH NĂNG CŨ (GIỮ NGUYÊN)
            // ==========================================
            
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
                var update = Builders<Poi>.Update.Inc(p => p.QrScans, 1);

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

            // ==========================================
            // 🔥 2. TÍNH NĂNG MỚI (NGƯỜI DÙNG ONLINE)
            // ==========================================

            // API 2.1: APP MOBILE BẮN NHỊP TIM (PING) LÊN SERVER
            app.MapPost("/api/tracking/ping", (string deviceId) =>
            {
                if (!string.IsNullOrEmpty(deviceId))
                {
                    // Cập nhật thời gian mới nhất thiết bị này hoạt động
                    _onlineUsers[deviceId] = DateTime.UtcNow;
                }
                return Results.Ok();
            });

            // API 2.2: WEB ADMIN LẤY SỐ LƯỢNG ONLINE THỰC TẾ
            app.MapGet("/admin/tracking/online", () =>
            {
                // THAY BẰNG DÒNG NÀY (Đợi 15 giây):
var timeout = DateTime.UtcNow.AddSeconds(-5);
                
                // Đếm số người còn hoạt động
                var activeCount = _onlineUsers.Count(x => x.Value >= timeout);

                // Dọn rác bộ nhớ (Xóa những máy đã offline cho nhẹ RAM Server)
                foreach (var user in _onlineUsers.Where(x => x.Value < timeout).ToList())
                {
                    _onlineUsers.TryRemove(user.Key, out _);
                }

                return Results.Ok(new { onlineCount = activeCount });
            });
        }
    }
}