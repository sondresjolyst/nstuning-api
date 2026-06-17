using Microsoft.EntityFrameworkCore;
using nstuning_api.Constants;
using nstuning_api.Infrastructure;
using nstuning_api.Models;

namespace nstuning_api.Features.Vehicles
{
    /// <summary>Manage the flat engine catalog and the factory-fitment links to models.</summary>
    public static class Engines
    {
        public static async Task<IResult> Create(EngineInput body, ApplicationDbContext db, CancellationToken ct)
        {
            var name = body.Name.Trim();
            if (body.BrandId is int brandId && !await db.CarBrands.AnyAsync(b => b.Id == brandId, ct))
                return TypedResults.NotFound();

            var existing = await db.CarEngines.FirstOrDefaultAsync(e => e.BrandId == body.BrandId && e.Name.ToLower() == name.ToLower(), ct);
            if (existing != null) return TypedResults.Ok(new EngineCatalogItem(existing.Id, existing.Name, existing.BrandId));

            var engine = new CarEngine { BrandId = body.BrandId, Name = name };
            db.CarEngines.Add(engine);
            await db.SaveChangesAsync(ct);
            return TypedResults.Ok(new EngineCatalogItem(engine.Id, engine.Name, engine.BrandId));
        }

        public static async Task<IResult> Rename(int id, VehicleName body, ApplicationDbContext db, CancellationToken ct)
        {
            var name = body.Name.Trim();
            var engine = await db.CarEngines.FindAsync([id], ct);
            if (engine == null) return TypedResults.NotFound();
            if (await db.CarEngines.AnyAsync(e => e.Id != id && e.BrandId == engine.BrandId && e.Name.ToLower() == name.ToLower(), ct))
                return TypedResults.Problem("An engine with that name already exists.", statusCode: StatusCodes.Status409Conflict);

            engine.Name = name;
            await db.SaveChangesAsync(ct);
            return TypedResults.Ok(new EngineCatalogItem(engine.Id, engine.Name, engine.BrandId));
        }

        public static async Task<IResult> Delete(int id, ApplicationDbContext db, CancellationToken ct)
        {
            var engine = await db.CarEngines.FindAsync([id], ct);
            if (engine == null) return TypedResults.NotFound();
            db.CarEngines.Remove(engine);
            await db.SaveChangesAsync(ct);
            return TypedResults.NoContent();
        }

        public static async Task<IResult> Link(int modelId, int engineId, ApplicationDbContext db, CancellationToken ct)
        {
            if (!await db.CarModels.AnyAsync(m => m.Id == modelId, ct)) return TypedResults.NotFound();
            if (!await db.CarEngines.AnyAsync(e => e.Id == engineId, ct)) return TypedResults.NotFound();

            if (!await db.CarModelEngines.AnyAsync(me => me.ModelId == modelId && me.EngineId == engineId, ct))
            {
                db.CarModelEngines.Add(new CarModelEngine { ModelId = modelId, EngineId = engineId });
                await db.SaveChangesAsync(ct);
            }
            return TypedResults.NoContent();
        }

        public static async Task<IResult> Unlink(int modelId, int engineId, ApplicationDbContext db, CancellationToken ct)
        {
            var link = await db.CarModelEngines.FirstOrDefaultAsync(me => me.ModelId == modelId && me.EngineId == engineId, ct);
            if (link == null) return TypedResults.NotFound();
            db.CarModelEngines.Remove(link);
            await db.SaveChangesAsync(ct);
            return TypedResults.NoContent();
        }

        public class Endpoints : IEndpoint
        {
            public void Map(IEndpointRouteBuilder app)
            {
                var group = app.MapGroup("/api/vehicles").RequireAuthorization(Policies.Admin);
                group.MapPost("engines", Create).WithValidation<EngineInput>();
                group.MapPut("engines/{id:int}", Rename).WithValidation<VehicleName>();
                group.MapDelete("engines/{id:int}", Delete);
                group.MapPost("models/{modelId:int}/engines/{engineId:int}", Link);
                group.MapDelete("models/{modelId:int}/engines/{engineId:int}", Unlink);
            }
        }
    }
}
