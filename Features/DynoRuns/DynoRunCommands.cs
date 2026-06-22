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
                DynoDate = dto.DynoDate,
                DisplacementCc = dto.DisplacementCc,
                AbsolutePressureKpa = dto.AbsolutePressureKpa,
                HubPowerBeforeWhp = dto.HubPowerBeforeWhp,
                HubPowerAfterWhp = dto.HubPowerAfterWhp,
                HubTorqueBeforeWnm = dto.HubTorqueBeforeWnm,
                HubTorqueAfterWnm = dto.HubTorqueAfterWnm,
                EnginePowerBeforeHp = dto.EnginePowerBeforeHp,
                EnginePowerAfterHp = dto.EnginePowerAfterHp,
                EngineTorqueBeforeNm = dto.EngineTorqueBeforeNm,
                EngineTorqueAfterNm = dto.EngineTorqueAfterNm,
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
            run.DynoDate = dto.DynoDate;
            run.DisplacementCc = dto.DisplacementCc;
            run.AbsolutePressureKpa = dto.AbsolutePressureKpa;
            run.HubPowerBeforeWhp = dto.HubPowerBeforeWhp;
            run.HubPowerAfterWhp = dto.HubPowerAfterWhp;
            run.HubTorqueBeforeWnm = dto.HubTorqueBeforeWnm;
            run.HubTorqueAfterWnm = dto.HubTorqueAfterWnm;
            run.EnginePowerBeforeHp = dto.EnginePowerBeforeHp;
            run.EnginePowerAfterHp = dto.EnginePowerAfterHp;
            run.EngineTorqueBeforeNm = dto.EngineTorqueBeforeNm;
            run.EngineTorqueAfterNm = dto.EngineTorqueAfterNm;
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
            // New reports keep the model default (now); re-uploads keep their original date.
            return report;
        }

        private static async Task ApplyCoverImageAsync(DynoRun run, IFormFile? cover, HttpContext http, ApplicationDbContext db, IImageStorageService images, CancellationToken ct)
        {
            if (cover == null || cover.Length == 0) return;

            var (storedPath, contentType, sizeBytes) = await images.SaveAsync(cover, ct);
            var oldImageId = run.CoverImageId;

            var newImage = new ContentImage
            {
                FileName = Path.GetFileName(cover.FileName),
                ContentType = contentType,
                SizeBytes = sizeBytes,
                StoredPath = storedPath,
                UploadedByUserId = http.User.UserId()
            };
            // Add explicitly: with a client-set key EF would otherwise track the
            // nav-attached entity as Unchanged and skip the insert.
            db.ContentImages.Add(newImage);
            run.CoverImage = newImage;

            var variants = await images.GenerateWebpVariantsAsync(storedPath, ct);
            foreach (var v in variants)
                newImage.Variants.Add(ContentImageVariant.From(newImage.Id, v));

            if (oldImageId != null)
            {
                var old = await db.ContentImages.Include(i => i.Variants).FirstOrDefaultAsync(i => i.Id == oldImageId, ct);
                if (old != null)
                {
                    images.Delete(old.StoredPath);
                    foreach (var v in old.Variants)
                        images.Delete(v.StoredPath);
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
