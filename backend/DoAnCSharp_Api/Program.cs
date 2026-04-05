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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

//API (MOBILE)
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
//VIEW
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
//ADD
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
static async Task<string> Translate(string text, string target)
{
    using var http = new HttpClient();

    var content = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("q", text),
        new KeyValuePair<string, string>("source", "vi"),
        new KeyValuePair<string, string>("target", target),
        new KeyValuePair<string, string>("format", "text")
    });

    var res = await http.PostAsync("https://libretranslate.de/translate", content);
    var json = await res.Content.ReadAsStringAsync();

    var doc = System.Text.Json.JsonDocument.Parse(json);
    return doc.RootElement.GetProperty("translatedText").GetString();
}
app.MapPost("/upload-image", async (HttpRequest request) =>
{
    var file = request.Form.Files[0];

    if (file == null || file.Length == 0)
        return Results.BadRequest("No file");

    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

    var path = Path.Combine("wwwroot/images", fileName);

    using var stream = new FileStream(path, FileMode.Create);
    await file.CopyToAsync(stream);

    return Results.Ok(new { fileName });
});
app.UseStaticFiles();
app.Run();