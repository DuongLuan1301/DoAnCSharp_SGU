using MongoDB.Driver;
using DoAnCSharp_Api.Models;
using System.Text.Json;

//These APIs are for admin website
public static class PoiEndpoints
{
    public static void MapPoiEndpoints(this WebApplication app)
    {
        //GET ALL POIs API
        app.MapGet("/admin/poi", async (IMongoCollection<Poi> poiCollection) =>
        {
            var pois = await poiCollection.Find(_ => true).ToListAsync();

            var result = pois.Select(p => new
            {
                id = p.Id.ToString(),
                p.Name,
                p.Address,
                p.Lat,
                p.Lng,
                p.Localizations,
                p.Image
            });

            return Results.Ok(result);
        });

        // ===== ADD POI =====
        app.MapPost("/admin/poi", async (
            Poi poi,
            IMongoCollection<Poi> poiCollection) =>
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
    private static readonly HttpClient _http = new HttpClient()
    {
        Timeout = TimeSpan.FromSeconds(10) // tăng thời gian chờ
    };

    private static async Task<string> Translate(string text, string target)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var url =
            $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=vi&tl={target}&dt=t&q={Uri.EscapeDataString(text)}";

        for (int attempt = 0; attempt < 3; attempt++)
        {
            try {
                var res = await _http.GetStringAsync(url);

                if (string.IsNullOrWhiteSpace(res) || res.StartsWith("<"))
                    continue;

                var json = JsonDocument.Parse(res);

                var translated = json.RootElement[0][0][0].GetString();

                if (!string.IsNullOrWhiteSpace(translated))
                    return translated;
            }
            catch {
                await Task.Delay(300 * (attempt + 1));
            }
        }
        // fallback nếu fail
        return text;
    }
}