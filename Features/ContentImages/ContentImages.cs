using Microsoft.EntityFrameworkCore;
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
        // Serializes lazy backfill so concurrent requests for an un-converted image don't double-generate.
        private static readonly SemaphoreSlim BackfillLock = new(1, 1);

        public static async Task<IResult> Get(string id, int? w, HttpContext http, ApplicationDbContext db, IImageStorageService storage, CancellationToken ct)
        {
            var image = await db.ContentImages.Include(i => i.Variants).FirstOrDefaultAsync(i => i.Id == id, ct);
            if (image == null || !storage.Exists(image.StoredPath))
                return TypedResults.NotFound();

            http.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
            http.Response.Headers.Vary = "Accept";

            var acceptsWebp = http.Request.Headers.Accept.ToString().Contains("image/webp", StringComparison.OrdinalIgnoreCase);
            if (acceptsWebp)
            {
                var variants = image.Variants.Count > 0
                    ? image.Variants.ToList()
                    : await EnsureVariantsAsync(image.Id, image.StoredPath, db, storage, ct);

                var chosen = PickVariant(variants, w);
                if (chosen != null && storage.Exists(chosen.StoredPath))
                    return TypedResults.File(storage.OpenRead(chosen.StoredPath), "image/webp");
            }

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

            await GenerateAndSaveVariantsAsync(image.Id, image.StoredPath, db, storage, ct);

            return TypedResults.Ok(new UploadedImage(image.Id, $"/content-images/{image.Id}"));
        }

        public static async Task<IResult> Delete(string id, ApplicationDbContext db, IImageStorageService storage, CancellationToken ct)
        {
            var image = await db.ContentImages.Include(i => i.Variants).FirstOrDefaultAsync(i => i.Id == id, ct);
            if (image == null) return TypedResults.NotFound();

            storage.Delete(image.StoredPath);
            foreach (var variant in image.Variants)
                storage.Delete(variant.StoredPath);

            db.ContentImages.Remove(image); // cascade removes variant rows
            await db.SaveChangesAsync(ct);
            return TypedResults.NoContent();
        }

        private static ContentImageVariant? PickVariant(IReadOnlyCollection<ContentImageVariant> variants, int? w)
        {
            var ordered = variants.OrderBy(v => v.Width).ToList();
            if (ordered.Count == 0) return null;
            if (w == null) return ordered[^1];
            return ordered.FirstOrDefault(v => v.Width >= w) ?? ordered[^1];
        }

        private static async Task<List<ContentImageVariant>> EnsureVariantsAsync(string imageId, string storedPath, ApplicationDbContext db, IImageStorageService storage, CancellationToken ct)
        {
            await BackfillLock.WaitAsync(ct);
            try
            {
                var existing = await db.ContentImageVariants.Where(v => v.ContentImageId == imageId).ToListAsync(ct);
                if (existing.Count > 0) return existing;
                return await GenerateAndSaveVariantsAsync(imageId, storedPath, db, storage, ct);
            }
            finally
            {
                BackfillLock.Release();
            }
        }

        private static async Task<List<ContentImageVariant>> GenerateAndSaveVariantsAsync(string imageId, string storedPath, ApplicationDbContext db, IImageStorageService storage, CancellationToken ct)
        {
            var generated = await storage.GenerateWebpVariantsAsync(storedPath, ct);
            var rows = generated
                .Select(g => new ContentImageVariant { ContentImageId = imageId, Width = g.Width, StoredPath = g.StoredPath, SizeBytes = g.SizeBytes })
                .ToList();
            if (rows.Count > 0)
            {
                db.ContentImageVariants.AddRange(rows);
                await db.SaveChangesAsync(ct);
            }
            return rows;
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
