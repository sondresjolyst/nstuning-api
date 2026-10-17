using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nstuning_api.Constants;
using nstuning_api.Helpers;
using nstuning_api.Infrastructure;
using nstuning_api.Models;
using nstuning_api.Services;
using SjolystInnovation.AppKit.Auth;
using SjolystInnovation.AppKit.Storage;
using SjolystInnovation.AppKit.Storage.Images;

namespace nstuning_api.Features.DynoRuns
{
    /// <summary>Admin create / update / delete for dyno runs (multipart with optional PDF + cover image).</summary>
    public static class DynoRunCommands
    {
        public static async Task<IResult> Create([FromForm] CreateDynoRunDto dto, HttpContext http, ApplicationDbContext db,
            [FromKeyedServices(ReportStore.Name)] IAppKitFileStore reports, IContentImageService images, IMapper mapper, CancellationToken ct)
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

            var (report, cover) = await StoreUploadsAsync(dto.Report, dto.CoverImage, http, reports, images, ct);
            if (report != null) run.Report = ToReport(report, http);
            if (cover != null) run.CoverImage = cover;

            db.DynoRuns.Add(run);
            await db.SaveChangesAsync(ct);
            return TypedResults.Created($"/api/dyno-runs/{run.Slug}", mapper.Map<DynoRunDto>(run));
        }

        public static async Task<IResult> Update(int id, [FromForm] UpdateDynoRunDto dto, HttpContext http, ApplicationDbContext db,
            [FromKeyedServices(ReportStore.Name)] IAppKitFileStore reports, IContentImageService images, IMapper mapper, CancellationToken ct)
        {
            var run = await db.DynoRuns.Include(d => d.Report).FirstOrDefaultAsync(d => d.Id == id, ct);
            if (run == null) return TypedResults.NotFound();

            // The cover's CreateAsync saves the context, so the uploads go in before the run changes.
            var (report, cover) = await StoreUploadsAsync(dto.Report, dto.CoverImage, http, reports, images, ct);

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

            var oldReportPath = report != null ? run.Report?.StoredPath : null;
            if (report != null) run.Report = ToReport(report, http, run.Report);
            var oldCoverId = cover != null ? run.CoverImageId : null;
            if (cover != null) run.CoverImage = cover;

            await db.SaveChangesAsync(ct);

            // The replaced files go once nothing points at them.
            if (oldReportPath != null) await reports.DeleteAsync(oldReportPath, CancellationToken.None);
            if (oldCoverId != null)
            {
                var old = await db.ContentImages.Include(i => i.Variants).FirstOrDefaultAsync(i => i.Id == oldCoverId, ct);
                if (old != null) await images.DeleteAsync(old, ct);
            }

            return TypedResults.Ok(mapper.Map<DynoRunDto>(run));
        }

        public static async Task<IResult> Delete(int id, ApplicationDbContext db,
            [FromKeyedServices(ReportStore.Name)] IAppKitFileStore reports, IContentImageService images, CancellationToken ct)
        {
            var run = await db.DynoRuns.Include(d => d.Report).Include(d => d.CoverImage).FirstOrDefaultAsync(d => d.Id == id, ct);
            if (run == null) return TypedResults.NotFound();

            var reportPath = run.Report?.StoredPath;
            var cover = run.CoverImage;

            db.DynoRuns.Remove(run);
            await db.SaveChangesAsync(ct);

            if (reportPath != null) await reports.DeleteAsync(reportPath, CancellationToken.None);
            if (cover != null) await images.DeleteAsync(cover, ct);
            return TypedResults.NoContent();
        }

        /// <summary>
        /// Stores the uploaded report file and cover image. A refused cover deletes the stored report again,
        /// so a refused request leaves nothing behind. The cover is saved to the database at once.
        /// </summary>
        private static async Task<(StoredFile? Report, ContentImage? Cover)> StoreUploadsAsync(IFormFile? reportFile, IFormFile? coverFile,
            HttpContext http, IAppKitFileStore reports, IContentImageService images, CancellationToken ct)
        {
            StoredFile? report = null;
            if (reportFile != null)
            {
                await using var content = reportFile.OpenReadStream();
                report = await reports.SaveAsync(content, reportFile.FileName, ct);
            }

            if (coverFile == null || coverFile.Length == 0) return (report, null);
            try
            {
                await using var content = coverFile.OpenReadStream();
                return (report, await images.CreateAsync(content, coverFile.FileName, http.User.GetUserId(), ct));
            }
            catch
            {
                if (report != null) await reports.DeleteAsync(report.StoredPath, CancellationToken.None);
                throw;
            }
        }

        private static DynoRunReport ToReport(StoredFile stored, HttpContext http, DynoRunReport? existing = null)
        {
            var report = existing ?? new DynoRunReport { FileName = "", ContentType = "application/pdf", StoredPath = "" };
            report.FileName = stored.FileName;
            report.ContentType = stored.ContentType;
            report.SizeBytes = stored.SizeBytes;
            report.StoredPath = stored.StoredPath;
            report.UploadedByUserId = http.User.GetUserId();
            // New reports keep the model default (now); re-uploads keep their original date.
            return report;
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
