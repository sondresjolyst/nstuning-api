using Microsoft.EntityFrameworkCore;
using nstuning_api.Constants;
using nstuning_api.Infrastructure;
using nstuning_api.Models;

namespace nstuning_api.Features.Vehicles
{
    /// <summary>Create / rename / delete a variant under a model.</summary>
    public static class Variants
    {
        public static async Task<IResult> Create(int modelId, VehicleName body, ApplicationDbContext db, CancellationToken ct)
        {
            var name = body.Name.Trim();
            if (!await db.CarModels.AnyAsync(m => m.Id == modelId, ct)) return TypedResults.NotFound();

            var existing = await db.CarVariants.FirstOrDefaultAsync(v => v.ModelId == modelId && v.Name.ToLower() == name.ToLower(), ct);
            if (existing != null) return TypedResults.Ok(new VehicleItem(existing.Id, existing.Name));

            var variant = new CarVariant { ModelId = modelId, Name = name };
            db.CarVariants.Add(variant);
            await db.SaveChangesAsync(ct);
            return TypedResults.Ok(new VehicleItem(variant.Id, variant.Name));
        }

        public static async Task<IResult> Rename(int id, VehicleName body, ApplicationDbContext db, CancellationToken ct)
        {
            var name = body.Name.Trim();
            var variant = await db.CarVariants.FindAsync([id], ct);
            if (variant == null) return TypedResults.NotFound();
            if (await db.CarVariants.AnyAsync(v => v.Id != id && v.ModelId == variant.ModelId && v.Name.ToLower() == name.ToLower(), ct))
                return TypedResults.Problem("A variant with that name already exists.", statusCode: StatusCodes.Status409Conflict);

            variant.Name = name;
            await db.SaveChangesAsync(ct);
            return TypedResults.Ok(new VehicleItem(variant.Id, variant.Name));
        }

        public static async Task<IResult> Delete(int id, ApplicationDbContext db, CancellationToken ct)
        {
            var variant = await db.CarVariants.FindAsync([id], ct);
            if (variant == null) return TypedResults.NotFound();
            db.CarVariants.Remove(variant);
            await db.SaveChangesAsync(ct);
            return TypedResults.NoContent();
        }

        public class Endpoints : IEndpoint
        {
            public void Map(IEndpointRouteBuilder app)
            {
                var group = app.MapGroup("/api/vehicles").RequireAuthorization(Policies.Admin);
                group.MapPost("models/{modelId:int}/variants", Create).WithValidation<VehicleName>();
                group.MapPut("variants/{id:int}", Rename).WithValidation<VehicleName>();
                group.MapDelete("variants/{id:int}", Delete);
            }
        }
    }
}
