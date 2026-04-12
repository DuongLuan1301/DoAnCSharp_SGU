using MongoDB.Driver;
using DoAnCSharp_Api.Models;
using System.Text.Json;
public static class PoiEndpoints
{
    public static void MapPoiEndpoints(this WebApplication app, IMongoCollection<Poi> poiCollection)
    {
        // ===== GET ALL POI =====
        app.MapGet("/admin/poi", async () =>
        {
            var pois = await poiCollection.Find(_ => true).ToListAsync();

            var result = pois.Select(p => new
            {
                id = p.Id.ToString(),
                p.Name,
                p.Address,
                p.Lat,
                p.Lng,
                p.Image,
                p.Localizations
            });

            return Results.Ok(result);
        });

        // ===== ADD POI =====
        app.MapPost("/admin/poi", async (Poi poi) =>
        {
            var viDesc = poi.Localizations?
                .FirstOrDefault(l => l.Lang == "vi")?.Description;

            if (string.IsNullOrEmpty(viDesc))
                return Results.BadRequest("Vietnamese description required");

            var en = await Translate(viDesc, "en");
            var ja = await Translate(viDesc, "ja");
            var zh = await Translate(viDesc, "zh");

            poi.Localizations = new List<Localization>
            {
                new() { Lang = "vi", Description = viDesc },
                new() { Lang = "en", Description = en },
                new() { Lang = "ja", Description = ja },
                new() { Lang = "zh", Description = zh }
            };

            await poiCollection.InsertOneAsync(poi);

            return Results.Ok();
        });
    }
    //AUTO TRANSLATE VIETNAMESE DESCRIPTION
    private static async Task<string> Translate(string text, string target)
    {
        try
        {
            using var http = new HttpClient();

            var url =
                $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=vi&tl={target}&dt=t&q={Uri.EscapeDataString(text)}";

            var res = await http.GetStringAsync(url);

            var json = JsonDocument.Parse(res);

            // lấy đoạn dịch
            return json.RootElement[0][0][0].GetString();
        }
        catch
        {
            return text;
        }
    }
}