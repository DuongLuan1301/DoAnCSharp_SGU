using MongoDB.Driver;
using DoAnCSharp_Api.Models;
using MongoDB.Bson;
using DoAnCSharp_Api.Endpoints; // Khai báo thư mục chứa Endpoints

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

// 1. Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// 2. Kết nối MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient("mongodb://localhost:27017"));

builder.Services.AddScoped<IMongoCollection<Poi>>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("poi_db").GetCollection<Poi>("pois");
});

builder.Services.AddScoped<IMongoCollection<User>>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase("poi_db").GetCollection<User>("users");
});

// 3. Build App
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 4. Đăng ký Middleware
app.UseCors("AllowAll");
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600");
    }
});

// ==========================================
// 5. GỌI TẤT CẢ CÁC MODULE API
// ==========================================
app.MapMobileEndpoints();
app.MapPoiEndpoints();
app.MapUploadEndpoints();
app.MapAuthEndpoints();
app.MapTrackingEndpoints(); 
app.MapUserEndpoints(); // Giữ lại dòng này để web Admin load được User

// 6. Chạy Server
app.Run("http://0.0.0.0:5188");