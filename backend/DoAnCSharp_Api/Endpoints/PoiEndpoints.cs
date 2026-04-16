using MongoDB.Driver;
using DoAnCSharp_Api.Models;
using System.Text.Json;

public static class PoiEndpoints
{
    public static void MapPoiEndpoints(this WebApplication app)
    {
        string baseUrl = "http://10.0.2.2:5188";

        // 1. GET ALL POIs
        app.MapGet("/admin/poi", async (IMongoCollection<Poi> poiCollection) =>
        {
            var pois = await poiCollection.Find(_ => true).ToListAsync();
            var result = pois.Select(p => new {
                id = p.Id.ToString(), clientId = p.ClientId, p.Name, p.Address, p.Lat, p.Lng, p.Localizations, p.Image, views = p.Views, qrScans = p.QrScans, audioListens = p.AudioListens
            });
            return Results.Ok(result);
        });

        // 2. GET POIs CỦA 1 CLIENT
        app.MapGet("/client/poi/{clientId}", async (string clientId, IMongoCollection<Poi> poiCollection) =>
        {
            var pois = await poiCollection.Find(p => p.ClientId == clientId).ToListAsync();
            var result = pois.Select(p => new {
                id = p.Id.ToString(), clientId = p.ClientId, p.Name, p.Address, p.Lat, p.Lng, p.Localizations, p.Image, views = p.Views, qrScans = p.QrScans, audioListens = p.AudioListens
            });
            return Results.Ok(result);
        });

        // 3. GET CHI TIẾT 1 POI
        app.MapGet("/api/poi/{id}", async (string id, IMongoCollection<Poi> poiCollection, string lang = "vi") =>
        {
            try {
                var p = await poiCollection.Find(poi => poi.Id == id.Trim()).FirstOrDefaultAsync();
                if (p == null) return Results.NotFound();

                string desc = "No description";
                if (p.Localizations != null && p.Localizations.Any()) {
                    var loc = p.Localizations.FirstOrDefault(l => l.Lang != null && l.Lang.Trim().ToLower() == lang.Trim().ToLower());
                    if (loc != null && !string.IsNullOrWhiteSpace(loc.Description)) desc = loc.Description;
                }

                string imgUrl = $"{baseUrl}/images/default.jpg?v={DateTime.Now.Ticks}";
                if (!string.IsNullOrWhiteSpace(p.Image)) {
                    if (p.Image.StartsWith("http")) imgUrl = p.Image;
                    else imgUrl = $"{baseUrl}/images/{p.Image}?v={DateTime.Now.Ticks}";
                }

                return Results.Ok(new { p.Id, p.ClientId, p.Name, p.Address, Image = imgUrl, p.Lat, p.Lng, Description = desc, p.Views, p.QrScans, p.AudioListens, p.Localizations });
            } catch { return Results.BadRequest("ID format error"); }
        });

        // 4. THÊM POI MỚI (AUTO DỊCH)
        app.MapPost("/admin/poi", async (Poi poi, IMongoCollection<Poi> poiCollection) =>
        {
            var viDesc = poi.Localizations?.FirstOrDefault(l => l.Lang == "vi")?.Description;
            if (string.IsNullOrEmpty(viDesc)) return Results.BadRequest("Vietnamese description required");

            var en = await Translate(viDesc, "en"); var ja = await Translate(viDesc, "ja"); var zh = await Translate(viDesc, "zh");

            poi.Localizations = new List<Localization> {
                new() { Lang = "vi", Description = viDesc }, new() { Lang = "en", Description = en }, new() { Lang = "ja", Description = ja }, new() { Lang = "zh", Description = zh }
            };
            await poiCollection.InsertOneAsync(poi);
            return Results.Ok();
        });

        // 5. UPDATE POI (CÓ BẢO MẬT)
        app.MapPut("/api/poi/{id}", async (string id, Poi updatedPoi, IMongoCollection<Poi> poiCollection) =>
        {
            try {
                updatedPoi.Id = id;
                if (!string.IsNullOrWhiteSpace(updatedPoi.Image)) {
                    string pathWithoutQuery = updatedPoi.Image.Split('?')[0];
                    updatedPoi.Image = System.IO.Path.GetFileName(pathWithoutQuery);
                }

                var oldPoi = await poiCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
                if (oldPoi == null) return Results.NotFound("Không tìm thấy POI");

                if (!string.IsNullOrEmpty(updatedPoi.ClientId) && oldPoi.ClientId != updatedPoi.ClientId)
                    return Results.BadRequest("Bạn không có quyền sửa gian hàng này!");

                if (string.IsNullOrEmpty(updatedPoi.ClientId)) updatedPoi.ClientId = oldPoi.ClientId;

                var result = await poiCollection.ReplaceOneAsync(p => p.Id == id, updatedPoi);
                return Results.Ok(new { message = "Cập nhật thành công" });
            } catch (Exception ex) { return Results.BadRequest(ex.Message); }
        });

        // 6. DELETE POI (CÓ BẢO MẬT)
        app.MapDelete("/admin/poi/{id}", async (string id, string? clientId, IMongoCollection<Poi> poiCollection) =>
        {
            try {
                DeleteResult result;
                if (!string.IsNullOrEmpty(clientId)) result = await poiCollection.DeleteOneAsync(p => p.Id == id.Trim() && p.ClientId == clientId.Trim());
                else result = await poiCollection.DeleteOneAsync(p => p.Id == id.Trim());

                if (result.DeletedCount > 0) return Results.Ok(new { message = "Xóa thành công" });
                return Results.BadRequest("Xóa thất bại: Bạn không có quyền xóa gian hàng này!");
            } catch (Exception ex) { return Results.BadRequest(new { message = "Lỗi hệ thống: " + ex.Message }); }
        });

        // 🔥 7. API ĐẶC QUYỀN: CẤP QUYỀN (GÁN POI)
        app.MapPut("/admin/poi/{id}/assign", async (string id, string clientId, IMongoCollection<Poi> poiCollection) =>
        {
            var update = Builders<Poi>.Update.Set(p => p.ClientId, clientId.Trim());
            var result = await poiCollection.UpdateOneAsync(p => p.Id == id.Trim(), update);

            if (result.MatchedCount > 0) return Results.Ok(new { message = "Cấp quyền thành công" });
            return Results.BadRequest("Không tìm thấy gian hàng");
        });
    }

    // AUTO TRANSLATE HÀM
    private static readonly HttpClient _http = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };
    private static async Task<string> Translate(string text, string target)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=vi&tl={target}&dt=t&q={Uri.EscapeDataString(text)}";
        for (int attempt = 0; attempt < 3; attempt++) {
            try {
                var res = await _http.GetStringAsync(url);
                if (string.IsNullOrWhiteSpace(res) || res.StartsWith("<")) continue;
                var json = JsonDocument.Parse(res);
                var translated = json.RootElement[0][0][0].GetString();
                if (!string.IsNullOrWhiteSpace(translated)) return translated;
            } catch { await Task.Delay(300 * (attempt + 1)); }
        }
        return text;
    }
}