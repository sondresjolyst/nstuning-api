using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using nstuning_api.Constants;
using nstuning_api.Dtos.DynoRun;
using nstuning_api.Helpers;
using nstuning_api.Models;
using nstuning_api.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace nstuning_api.Controllers
{
    /// <summary>
    /// Public dyno-run showcase plus admin-gated create/update/delete.
    /// </summary>
    [ApiController]
    [Route("api/dyno-runs")]
    [EnableCors("AllowAllOrigins")]
    public class DynoRunsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IReportStorageService _storage;
        private readonly IMapper _mapper;
        private readonly ILogger<DynoRunsController> _logger;

        public DynoRunsController(
            ApplicationDbContext context,
            IReportStorageService storage,
            IMapper mapper,
            ILogger<DynoRunsController> logger)
        {
            _context = context;
            _storage = storage;
            _mapper = mapper;
            _logger = logger;
        }

        private bool IsAdmin() => User.IsInRole(RoleNames.Admin);

        /// <summary>
        /// Lists dyno runs. Published only for anonymous callers; admins may pass all=true to include drafts.
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Lists dyno runs.")]
        public async Task<IActionResult> GetAll([FromQuery] bool all = false, CancellationToken ct = default)
        {
            var includeDrafts = all && IsAdmin();
            var query = _context.DynoRuns.AsNoTracking().Include(d => d.Report).AsQueryable();
            if (!includeDrafts)
                query = query.Where(d => d.Published);

            var runs = await query
                .OrderBy(d => d.SortOrder).ThenByDescending(d => d.CreatedAt)
                .ToListAsync(ct);

            return Ok(runs.Select(_mapper.Map<DynoRunDto>));
        }

        /// <summary>
        /// Gets a single dyno run by slug.
        /// </summary>
        [HttpGet("{slug}")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Gets a single dyno run by slug.")]
        public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct = default)
        {
            var run = await _context.DynoRuns.AsNoTracking()
                .Include(d => d.Report)
                .FirstOrDefaultAsync(d => d.Slug == slug, ct);

            if (run == null || (!run.Published && !IsAdmin()))
                return NotFound();

            return Ok(_mapper.Map<DynoRunDto>(run));
        }

        /// <summary>
        /// Streams the PDF report for a dyno run inline.
        /// </summary>
        [HttpGet("{id:int}/report")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Streams the PDF report inline.")]
        public async Task<IActionResult> GetReport(int id, CancellationToken ct = default)
        {
            var run = await _context.DynoRuns.AsNoTracking()
                .Include(d => d.Report)
                .FirstOrDefaultAsync(d => d.Id == id, ct);

            if (run == null || (!run.Published && !IsAdmin()) || run.Report == null)
                return NotFound();

            if (!_storage.Exists(run.Report.StoredPath))
            {
                _logger.LogWarning("Report file missing on disk for dyno run {Id}", id);
                return NotFound();
            }

            var stream = _storage.OpenRead(run.Report.StoredPath);
            Response.Headers.ContentDisposition = $"inline; filename=\"{run.Report.FileName}\"";
            return File(stream, run.Report.ContentType);
        }

        /// <summary>
        /// Creates a new dyno run (admin only). multipart/form-data with optional PDF report and cover image.
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "Admin")]
        [SwaggerOperation(Summary = "Creates a new dyno run.")]
        public async Task<IActionResult> Create([FromForm] CreateDynoRunDto dto, CancellationToken ct = default)
        {
            var run = new DynoRun
            {
                Title = dto.Title,
                Slug = await UniqueSlugAsync(dto.Title, null, ct),
                CarMake = dto.CarMake,
                CarModel = dto.CarModel,
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

            await ApplyCoverImageAsync(run, dto.CoverImage, ct);

            if (dto.Report != null)
            {
                var storedName = await _storage.SaveAsync(dto.Report, ct);
                run.Report = new DynoRunReport
                {
                    FileName = Path.GetFileName(dto.Report.FileName),
                    ContentType = "application/pdf",
                    SizeBytes = dto.Report.Length,
                    StoredPath = storedName,
                    UploadedByUserId = User.UserId()
                };
            }

            _context.DynoRuns.Add(run);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Dyno run created {Id} by {UserId}", run.Id, User.UserId());
            return CreatedAtAction(nameof(GetBySlug), new { slug = run.Slug }, _mapper.Map<DynoRunDto>(run));
        }

        /// <summary>
        /// Updates a dyno run (admin only). Optionally replaces the PDF report and/or cover image.
        /// </summary>
        [HttpPut("{id:int}")]
        [Authorize(Policy = "Admin")]
        [SwaggerOperation(Summary = "Updates a dyno run.")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateDynoRunDto dto, CancellationToken ct = default)
        {
            var run = await _context.DynoRuns.Include(d => d.Report).FirstOrDefaultAsync(d => d.Id == id, ct);
            if (run == null)
                return NotFound();

            if (!string.Equals(run.Title, dto.Title, StringComparison.Ordinal))
                run.Slug = await UniqueSlugAsync(dto.Title, run.Id, ct);

            run.Title = dto.Title;
            run.CarMake = dto.CarMake;
            run.CarModel = dto.CarModel;
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

            await ApplyCoverImageAsync(run, dto.CoverImage, ct);

            if (dto.Report != null)
            {
                var storedName = await _storage.SaveAsync(dto.Report, ct);
                if (run.Report != null)
                {
                    _storage.Delete(run.Report.StoredPath);
                    run.Report.FileName = Path.GetFileName(dto.Report.FileName);
                    run.Report.ContentType = "application/pdf";
                    run.Report.SizeBytes = dto.Report.Length;
                    run.Report.StoredPath = storedName;
                    run.Report.UploadedByUserId = User.UserId();
                    run.Report.CreatedAt = DateTime.UtcNow;
                }
                else
                {
                    run.Report = new DynoRunReport
                    {
                        FileName = Path.GetFileName(dto.Report.FileName),
                        ContentType = "application/pdf",
                        SizeBytes = dto.Report.Length,
                        StoredPath = storedName,
                        UploadedByUserId = User.UserId()
                    };
                }
            }

            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Dyno run updated {Id} by {UserId}", run.Id, User.UserId());
            return Ok(_mapper.Map<DynoRunDto>(run));
        }

        /// <summary>
        /// Deletes a dyno run and its stored report (admin only).
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Policy = "Admin")]
        [SwaggerOperation(Summary = "Deletes a dyno run.")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
        {
            var run = await _context.DynoRuns.Include(d => d.Report).FirstOrDefaultAsync(d => d.Id == id, ct);
            if (run == null)
                return NotFound();

            if (run.Report != null)
                _storage.Delete(run.Report.StoredPath);

            _context.DynoRuns.Remove(run);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Dyno run deleted {Id} by {UserId}", id, User.UserId());
            return NoContent();
        }

        private static async Task ApplyCoverImageAsync(DynoRun run, IFormFile? cover, CancellationToken ct)
        {
            if (cover == null || cover.Length == 0)
                return;
            if (!cover.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Cover image must be an image file.");

            using var ms = new MemoryStream();
            await cover.CopyToAsync(ms, ct);
            run.CoverImageData = Convert.ToBase64String(ms.ToArray());
            run.CoverImageContentType = cover.ContentType;
        }

        private async Task<string> UniqueSlugAsync(string title, int? excludeId, CancellationToken ct)
        {
            var baseSlug = Slugify.Create(title);
            if (string.IsNullOrEmpty(baseSlug))
                baseSlug = "dyno-run";

            var slug = baseSlug;
            var suffix = 2;
            while (await _context.DynoRuns.AnyAsync(d => d.Slug == slug && d.Id != excludeId, ct))
            {
                slug = $"{baseSlug}-{suffix}";
                suffix++;
            }
            return slug;
        }
    }
}
