using MongoDB.Driver;
using DoAnCSharp_Api.Models;
using MongoDB.Bson;
using DoAnCSharp_Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

// 1. Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
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
app.UseCors("AllowAll");
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 4. Đăng ký Middleware;
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true,
    DefaultContentType = "application/octet-stream"
});

// ==========================================
// 5. GỌI TẤT CẢ CÁC MODULE API
// ==========================================
app.MapMobileEndpoints();
app.MapPoiEndpoints();
app.MapUploadEndpoints();
app.MapAuthEndpoints();
app.MapTrackingEndpoints();
app.MapUserEndpoints();

// 6. Chạy Server
app.Run("http://0.0.0.0:5188");