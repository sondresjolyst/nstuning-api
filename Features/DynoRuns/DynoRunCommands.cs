using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nstuning_api.Constants;
using nstuning_api.Helpers;
using nstuning_api.Infrastructure;
using nstuning_api.Models;
using nstuning_api.Services;

namespace nstuning_api.Features.DynoRuns
{
    /// <summary>Admin create / update / delete for dyno runs (multipart with optional PDF + cover image).</summary>
    public static class DynoRunCommands
    {
        public static async Task<IResult> Create([FromForm] CreateDynoRunDto dto, HttpContext http, ApplicationDbContext db,
            IReportStorageService reports, IImageStorageService images, IMapper mapper, CancellationToken ct)
        {
            var run = new DynoRun
            {
                Title = dto.Title,
                Slug = await UniqueSlugAsync(dto.Title, null, db, ct),
                CarMake = dto.CarMake,
                CarModel = dto.CarModel,
                Trim = dto.Trim,
                Year = dto.Year,
                Engine = dto.Engine,
                FuelType = dto.FuelType,
                PowerBeforeHp = dto.PowerBeforeHp,
                PowerAfterHp = dto.PowerAfterHp,
                TorqueBeforeNm = dto.TorqueBeforeNm,
                TorqueAfterNm = dto.TorqueAfterNm,
                Description = dto.Description,
                Published = dto.Published,
                SortOrder = dto.SortOrder,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await ApplyCoverImageAsync(run, dto.CoverImage, http, db, images, ct);
            if (dto.Report != null)
                run.Report = await SaveReportAsync(dto.Report, http, reports, ct);

            db.DynoRuns.Add(run);
            await db.SaveChangesAsync(ct);
            return TypedResults.Created($"/api/dyno-runs/{run.Slug}", mapper.Map<DynoRunDto>(run));
        }

        public static async Task<IResult> Update(int id, [FromForm] UpdateDynoRunDto dto, HttpContext http, ApplicationDbContext db,
            IReportStorageService reports, IImageStorageService images, IMapper mapper, CancellationToken ct)
        {
            var run = await db.DynoRuns.Include(d => d.Report).FirstOrDefaultAsync(d => d.Id == id, ct);
            if (run == null) return TypedResults.NotFound();

            if (!string.Equals(run.Title, dto.Title, StringComparison.Ordinal))
                run.Slug = await UniqueSlugAsync(dto.Title, run.Id, db, ct);

            run.Title = dto.Title;
            run.CarMake = dto.CarMake;
            run.CarModel = dto.CarModel;
            run.Trim = dto.Trim;
            run.Year = dto.Year;
            run.Engine = dto.Engine;
            run.FuelType = dto.FuelType;
            run.PowerBeforeHp = dto.PowerBeforeHp;
            run.PowerAfterHp = dto.PowerAfterHp;
            run.TorqueBeforeNm = dto.TorqueBeforeNm;
            run.TorqueAfterNm = dto.TorqueAfterNm;
            run.Description = dto.Description;
            run.Published = dto.Published;
            run.SortOrder = dto.SortOrder;
            run.UpdatedAt = DateTime.UtcNow;

            await ApplyCoverImageAsync(run, dto.CoverImage, http, db, images, ct);

            if (dto.Report != null)
            {
                if (run.Report != null) reports.Delete(run.Report.StoredPath);
                run.Report = await SaveReportAsync(dto.Report, http, reports, ct, run.Report);
            }

            await db.SaveChangesAsync(ct);
            return TypedResults.Ok(mapper.Map<DynoRunDto>(run));
        }

        public static async Task<IResult> Delete(int id, ApplicationDbContext db, IReportStorageService reports, IImageStorageService images, CancellationToken ct)
        {
            var run = await db.DynoRuns.Include(d => d.Report).Include(d => d.CoverImage).FirstOrDefaultAsync(d => d.Id == id, ct);
            if (run == null) return TypedResults.NotFound();

            if (run.Report != null) reports.Delete(run.Report.StoredPath);
            if (run.CoverImage != null)
            {
                images.Delete(run.CoverImage.StoredPath);
                db.ContentImages.Remove(run.CoverImage);
            }

            db.DynoRuns.Remove(run);
            await db.SaveChangesAsync(ct);
            return TypedResults.NoContent();
        }

        private static async Task<DynoRunReport> SaveReportAsync(IFormFile file, HttpContext http, IReportStorageService reports, CancellationToken ct, DynoRunReport? existing = null)
        {
            var storedName = await reports.SaveAsync(file, ct);
            var report = existing ?? new DynoRunReport { FileName = "", ContentType = "application/pdf", StoredPath = "" };
            report.FileName = Path.GetFileName(file.FileName);
            report.ContentType = "application/pdf";
            report.SizeBytes = file.Length;
            report.StoredPath = storedName;
            report.UploadedByUserId = http.User.UserId();
            report.CreatedAt = DateTime.UtcNow;
            return report;
        }

        private static async Task ApplyCoverImageAsync(DynoRun run, IFormFile? cover, HttpContext http, ApplicationDbContext db, IImageStorageService images, CancellationToken ct)
        {
            if (cover == null || cover.Length == 0) return;

            var (storedPath, contentType, sizeBytes) = await images.SaveAsync(cover, ct);
            var oldImageId = run.CoverImageId;

            run.CoverImage = new ContentImage
            {
                FileName = Path.GetFileName(cover.FileName),
                ContentType = contentType,
                SizeBytes = sizeBytes,
                StoredPath = storedPath,
                UploadedByUserId = http.User.UserId()
            };

            if (oldImageId != null)
            {
                var old = await db.ContentImages.FindAsync([oldImageId], ct);
                if (old != null)
                {
                    images.Delete(old.StoredPath);
                    db.ContentImages.Remove(old);
                }
            }
        }

        private static async Task<string> UniqueSlugAsync(string title, int? excludeId, ApplicationDbContext db, CancellationToken ct)
        {
            var baseSlug = Slugify.Create(title);
            if (string.IsNullOrEmpty(baseSlug)) baseSlug = "dyno-run";

            var slug = baseSlug;
            var suffix = 2;
            while (await db.DynoRuns.AnyAsync(d => d.Slug == slug && d.Id != excludeId, ct))
            {
                slug = $"{baseSlug}-{suffix}";
                suffix++;
            }
            return slug;
        }

        public class Endpoints : IEndpoint
        {
            public void Map(IEndpointRouteBuilder app)
            {
                var group = app.MapGroup("/api/dyno-runs").RequireAuthorization(Policies.Admin).DisableAntiforgery();
                group.MapPost("", Create).WithValidation<CreateDynoRunDto>();
                group.MapPut("{id:int}", Update).WithValidation<UpdateDynoRunDto>();
                group.MapDelete("{id:int}", Delete);
            }
        }
    }
}
