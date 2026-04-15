using MongoDB.Driver;
using DoAnCSharp_Api.Models;
//These APIs are for mobile app
public static class MobileEndpoints
{
    public static void MapMobileEndpoints(this WebApplication app, IMongoCollection<Poi> poiCollection)
    {
        //GET ALL POIs API
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
    }
}