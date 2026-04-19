public static class UploadEndpoints
{
    public static void MapUploadEndpoints(this WebApplication app)
    {
        app.MapPost("/upload-image", async (HttpRequest request) =>
        {
            if (!request.HasFormContentType)
                return Results.BadRequest("Invalid form");

            var form = await request.ReadFormAsync();
            var file = form.Files.FirstOrDefault();

            if (file == null || file.Length == 0)
                return Results.BadRequest("No file");

            var folder = Path.Combine("wwwroot", "images");
            Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine(folder, fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return Results.Ok(new { fileName });
        });
    }
}