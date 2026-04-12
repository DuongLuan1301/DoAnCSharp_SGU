using MongoDB.Driver;
using DoAnCSharp_Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var client = new MongoClient("mongodb://localhost:27017");
var database = client.GetDatabase("poi_db");
var poiCollection = database.GetCollection<Poi>("pois");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        p => p.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();
app.UseCors("AllowAll");
app.UseStaticFiles();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
// app.UseHttpsRedirection();

// gọi endpoints
app.MapMobileEndpoints(poiCollection);
app.MapPoiEndpoints(poiCollection);
app.MapUploadEndpoints();

app.Run();