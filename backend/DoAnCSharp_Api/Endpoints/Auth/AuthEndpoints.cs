using MongoDB.Driver;
using DoAnCSharp_Api.Models;
using BCrypt.Net;
using System.Text.Json;

public static class AuthEndpoints
{
    //These APIs are for client website
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        // ================= REGISTER =================
        group.MapPost("/register", async (UserRegisterDto dto, IMongoCollection<User> users) =>
        {
            // check email và sđt tồn tại
            var exist = await users.Find(x => x.Email == dto.Email || x.Phone == dto.Phone).FirstOrDefaultAsync();
            if (exist != null)
                return Results.BadRequest(new { message = "Email hoặc SĐT đã tồn tại" });

            // hash password
            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Password = hash,
                Status = "active"
            };

            await users.InsertOneAsync(user);

            return Results.Ok(new { message = "Đăng ký thành công" });
        });

        // ================= LOGIN =================
        // group.MapPost("/login", async (UserLoginDto dto, IMongoCollection<User> users) =>
        // {
        //     var user = await users.Find(x => x.Email == dto.Email).FirstOrDefaultAsync();

        //     if (user == null)
        //         return Results.BadRequest("Sai email");

        //     bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

        //     if (!isValid)
        //         return Results.BadRequest("Sai mật khẩu");

        //     // 🔥 Tạm thời chưa cần JWT
        //     return Results.Ok(new
        //     {
        //         user.Id,
        //         user.Name,
        //         user.Email
        //     });
        // });
    }
}