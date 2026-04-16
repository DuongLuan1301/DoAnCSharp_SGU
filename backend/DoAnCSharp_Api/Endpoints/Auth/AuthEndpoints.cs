using MongoDB.Driver;
using DoAnCSharp_Api.Models;
using BCrypt.Net;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        // ================= REGISTER =================
        group.MapPost("/register", async (UserRegisterDto dto, IMongoCollection<User> users) =>
        {
            var exist = await users.Find(x => x.Email == dto.Email || x.Phone == dto.Phone).FirstOrDefaultAsync();
            if (exist != null) return Results.BadRequest(new { message = "Email hoặc SĐT đã tồn tại" });

            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var user = new User
            {
                Name = dto.Name, Email = dto.Email, Phone = dto.Phone,
                Password = hash, Status = "active"
            };
            await users.InsertOneAsync(user);
            return Results.Ok(new { message = "Đăng ký thành công" });
        });

        // ================= LOGIN =================
        group.MapPost("/login", async (UserLoginDto dto, IMongoCollection<User> users) =>
        {
            var user = await users.Find(x => x.Email == dto.Email).FirstOrDefaultAsync();
            if (user == null) return Results.BadRequest(new { message = "Sai email" });

            if (user.Status == "locked") return Results.BadRequest(new { message = "Tài khoản của bạn đã bị khóa!" });

            bool isValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);
            if (!isValid) return Results.BadRequest(new { message = "Sai mật khẩu" });

            return Results.Ok(new
            {
                id = user.Id.ToString(), // Trả về ID của Client
                name = user.Name,
                email = user.Email
            });
        });
    }
}

// Thêm class DTO để nhận dữ liệu đăng nhập
public class UserLoginDto
{
    public string Email { get; set; }
    public string Password { get; set; }
}