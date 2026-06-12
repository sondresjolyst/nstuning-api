using nstuning_api.Constants;
using nstuning_api.Helpers;
using nstuning_api.Infrastructure;
using nstuning_api.Models;
using nstuning_api.Services;

namespace nstuning_api.Features.ContentImages
{
    public record UploadedImage(string Id, string Url);

    /// <summary>Upload / serve / delete owner images used in site content.</summary>
    public static class ContentImages
    {
        public static async Task<IResult> Get(string id, HttpContext http, ApplicationDbContext db, IImageStorageService storage, CancellationToken ct)
        {
            var image = await db.ContentImages.FindAsync([id], ct);
            if (image == null || !storage.Exists(image.StoredPath))
                return TypedResults.NotFound();

            http.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
            return TypedResults.File(storage.OpenRead(image.StoredPath), image.ContentType);
        }

        public static async Task<IResult> Upload(IFormFile file, HttpContext http, ApplicationDbContext db, IImageStorageService storage, CancellationToken ct)
        {
            var (storedPath, contentType, sizeBytes) = await storage.SaveAsync(file, ct);

            var image = new ContentImage
            {
                FileName = Path.GetFileName(file.FileName),
                ContentType = contentType,
                SizeBytes = sizeBytes,
                StoredPath = storedPath,
                UploadedByUserId = http.User.UserId()
            };
            db.ContentImages.Add(image);
            await db.SaveChangesAsync(ct);

            return TypedResults.Ok(new UploadedImage(image.Id, $"/content-images/{image.Id}"));
        }

        public static async Task<IResult> Delete(string id, ApplicationDbContext db, IImageStorageService storage, CancellationToken ct)
        {
            var image = await db.ContentImages.FindAsync([id], ct);
            if (image == null) return TypedResults.NotFound();

            storage.Delete(image.StoredPath);
            db.ContentImages.Remove(image);
            await db.SaveChangesAsync(ct);
            return TypedResults.NoContent();
        }

        public class Endpoints : IEndpoint
        {
            public void Map(IEndpointRouteBuilder app)
            {
                app.MapGet("/api/content-images/{id}", Get);
                app.MapPost("/api/content-images", Upload).RequireAuthorization(Policies.Admin).DisableAntiforgery();
                app.MapDelete("/api/content-images/{id}", Delete).RequireAuthorization(Policies.Admin);
            }
        }
    }
}
