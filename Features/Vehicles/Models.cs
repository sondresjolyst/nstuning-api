using Microsoft.EntityFrameworkCore;
using nstuning_api.Constants;
using nstuning_api.Infrastructure;
using nstuning_api.Models;

namespace nstuning_api.Features.Vehicles
{
    /// <summary>Create / rename / delete a model under a brand.</summary>
    public static class CarModels
    {
        public static async Task<IResult> Create(int brandId, VehicleName body, ApplicationDbContext db, CancellationToken ct)
        {
            var name = body.Name.Trim();
            if (!await db.CarBrands.AnyAsync(b => b.Id == brandId, ct)) return TypedResults.NotFound();

            var existing = await db.CarModels.FirstOrDefaultAsync(m => m.BrandId == brandId && m.Name.ToLower() == name.ToLower(), ct);
            if (existing != null) return TypedResults.Ok(new VehicleItem(existing.Id, existing.Name));

            var model = new CarModel { BrandId = brandId, Name = name };
            db.CarModels.Add(model);
            await db.SaveChangesAsync(ct);
            return TypedResults.Ok(new VehicleItem(model.Id, model.Name));
        }

        public static async Task<IResult> SetFamily(int id, ModelFamily body, ApplicationDbContext db, CancellationToken ct)
        {
            var model = await db.CarModels.FindAsync([id], ct);
            if (model == null) return TypedResults.NotFound();

            var family = body.Family?.Trim();
            model.Family = string.IsNullOrEmpty(family) ? null : family;
            await db.SaveChangesAsync(ct);
            return TypedResults.Ok(new VehicleItem(model.Id, model.Name));
        }

        public static async Task<IResult> Rename(int id, VehicleName body, ApplicationDbContext db, CancellationToken ct)
        {
            var name = body.Name.Trim();
            var model = await db.CarModels.FindAsync([id], ct);
            if (model == null) return TypedResults.NotFound();
            if (await db.CarModels.AnyAsync(m => m.Id != id && m.BrandId == model.BrandId && m.Name.ToLower() == name.ToLower(), ct))
                return TypedResults.Problem("A model with that name already exists.", statusCode: StatusCodes.Status409Conflict);

            model.Name = name;
            await db.SaveChangesAsync(ct);
            return TypedResults.Ok(new VehicleItem(model.Id, model.Name));
        }

        public static async Task<IResult> Delete(int id, ApplicationDbContext db, CancellationToken ct)
        {
            var model = await db.CarModels.FindAsync([id], ct);
            if (model == null) return TypedResults.NotFound();
            db.CarModels.Remove(model);
            await db.SaveChangesAsync(ct);
            return TypedResults.NoContent();
        }

        public class Endpoints : IEndpoint
        {
            public void Map(IEndpointRouteBuilder app)
            {
                var group = app.MapGroup("/api/vehicles").RequireAuthorization(Policies.Admin);
                group.MapPost("brands/{brandId:int}/models", Create).WithValidation<VehicleName>();
                group.MapPut("models/{id:int}", Rename).WithValidation<VehicleName>();
                group.MapPut("models/{id:int}/family", SetFamily).WithValidation<ModelFamily>();
                group.MapDelete("models/{id:int}", Delete);
            }
        }
    }
}
