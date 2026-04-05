using MongoDB.Driver;
using DoAnCSharp_Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var client = new MongoClient("mongodb://localhost:27017");
var database = client.GetDatabase("poi_db");
var poiCollection = database.GetCollection<Poi>("pois");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

//API
app.MapGet("/api/poi", async (string lang) =>
{
    var pois = await poiCollection.Find(_ => true).ToListAsync();

    var result = pois.Select(p => new
    {
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
app.UseStaticFiles();
app.Run();