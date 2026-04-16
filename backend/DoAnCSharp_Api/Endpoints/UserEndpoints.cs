using MongoDB.Driver;
using DoAnCSharp_Api.Models;

namespace DoAnCSharp_Api.Endpoints 
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this WebApplication app)
        {
            // 1. LẤY DANH SÁCH TÀI KHOẢN (Đổ lên bảng)
            app.MapGet("/admin/users", async (IMongoCollection<User> users) =>
            {
                var allUsers = await users.Find(_ => true).ToListAsync();
                return Results.Ok(allUsers);
            });

            // 2. XÓA TÀI KHOẢN
            app.MapDelete("/admin/users/{id}", async (string id, IMongoCollection<User> users) =>
            {
                await users.DeleteOneAsync(u => u.Id == id);
                return Results.Ok();
            });

            // 3. CẬP NHẬT TÀI KHOẢN (Sửa thông tin)
            app.MapPut("/admin/users/{id}", async (string id, User updated, IMongoCollection<User> users) =>
            {
                var updateDef = Builders<User>.Update
                    .Set(u => u.Name, updated.Name)
                    .Set(u => u.Phone, updated.Phone)
                    .Set(u => u.Email, updated.Email);

                // Nếu Admin nhập mật khẩu mới ở Form thì mã hóa (Hash) và lưu lại
                if (!string.IsNullOrEmpty(updated.Password))
                {
                    var hash = BCrypt.Net.BCrypt.HashPassword(updated.Password);
                    updateDef = updateDef.Set(u => u.Password, hash);
                }

                await users.UpdateOneAsync(u => u.Id == id, updateDef);
                return Results.Ok();
            });

            // 4. ĐỔI TRẠNG THÁI (Khóa / Mở khóa tài khoản)
            app.MapPut("/admin/users/{id}/status", async (string id, IMongoCollection<User> users) =>
            {
                var user = await users.Find(u => u.Id == id).FirstOrDefaultAsync();
                if (user == null) return Results.NotFound();

                var newStatus = user.Status == "locked" ? "active" : "locked";
                await users.UpdateOneAsync(u => u.Id == id, Builders<User>.Update.Set(u => u.Status, newStatus));
                return Results.Ok();
            });
        }
    }
}