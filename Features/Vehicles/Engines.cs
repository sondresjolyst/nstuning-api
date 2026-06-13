using Microsoft.EntityFrameworkCore;
using nstuning_api.Constants;
using nstuning_api.Infrastructure;
using nstuning_api.Models;

namespace nstuning_api.Features.Vehicles
{
    /// <summary>Create / rename / delete an engine under a variant.</summary>
    public static class Engines
    {
        public static async Task<IResult> Create(int variantId, VehicleName body, ApplicationDbContext db, CancellationToken ct)
        {
            var name = body.Name.Trim();
            if (!await db.CarVariants.AnyAsync(v => v.Id == variantId, ct)) return TypedResults.NotFound();

            var existing = await db.CarEngines.FirstOrDefaultAsync(e => e.VariantId == variantId && e.Name.ToLower() == name.ToLower(), ct);
            if (existing != null) return TypedResults.Ok(new VehicleItem(existing.Id, existing.Name));

            var engine = new CarEngine { VariantId = variantId, Name = name };
            db.CarEngines.Add(engine);
            await db.SaveChangesAsync(ct);
            return TypedResults.Ok(new VehicleItem(engine.Id, engine.Name));
        }

        public static async Task<IResult> Rename(int id, VehicleName body, ApplicationDbContext db, CancellationToken ct)
        {
            var name = body.Name.Trim();
            var engine = await db.CarEngines.FindAsync([id], ct);
            if (engine == null) return TypedResults.NotFound();
            if (await db.CarEngines.AnyAsync(e => e.Id != id && e.VariantId == engine.VariantId && e.Name.ToLower() == name.ToLower(), ct))
                return TypedResults.Problem("An engine with that name already exists.", statusCode: StatusCodes.Status409Conflict);

            engine.Name = name;
            await db.SaveChangesAsync(ct);
            return TypedResults.Ok(new VehicleItem(engine.Id, engine.Name));
        }

        public static async Task<IResult> Delete(int id, ApplicationDbContext db, CancellationToken ct)
        {
            var engine = await db.CarEngines.FindAsync([id], ct);
            if (engine == null) return TypedResults.NotFound();
            db.CarEngines.Remove(engine);
            await db.SaveChangesAsync(ct);
            return TypedResults.NoContent();
        }

        public class Endpoints : IEndpoint
        {
            public void Map(IEndpointRouteBuilder app)
            {
                var group = app.MapGroup("/api/vehicles").RequireAuthorization(Policies.Admin);
                group.MapPost("variants/{variantId:int}/engines", Create).WithValidation<VehicleName>();
                group.MapPut("engines/{id:int}", Rename).WithValidation<VehicleName>();
                group.MapDelete("engines/{id:int}", Delete);
            }
        }
    }
}
