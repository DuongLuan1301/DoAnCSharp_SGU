public static class UploadEndpoints
{
    public static void MapUploadEndpoints(this WebApplication app)
    {
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
    }
}