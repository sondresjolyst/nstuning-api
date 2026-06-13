using Microsoft.EntityFrameworkCore;
using nstuning_api.Constants;
using nstuning_api.Infrastructure;
using nstuning_api.Models;

namespace nstuning_api.Features.Vehicles
{
    /// <summary>Create / rename / delete a car brand.</summary>
    public static class Brands
    {
        public static async Task<IResult> Create(VehicleName body, ApplicationDbContext db, CancellationToken ct)
        {
            var name = body.Name.Trim();
            var existing = await db.CarBrands.FirstOrDefaultAsync(b => b.Name.ToLower() == name.ToLower(), ct);
            if (existing != null) return TypedResults.Ok(new VehicleItem(existing.Id, existing.Name));

            var brand = new CarBrand { Name = name };
            db.CarBrands.Add(brand);
            await db.SaveChangesAsync(ct);
            return TypedResults.Ok(new VehicleItem(brand.Id, brand.Name));
        }

        public static async Task<IResult> Rename(int id, VehicleName body, ApplicationDbContext db, CancellationToken ct)
        {
            var name = body.Name.Trim();
            var brand = await db.CarBrands.FindAsync([id], ct);
            if (brand == null) return TypedResults.NotFound();
            if (await db.CarBrands.AnyAsync(b => b.Id != id && b.Name.ToLower() == name.ToLower(), ct))
                return TypedResults.Problem("A brand with that name already exists.", statusCode: StatusCodes.Status409Conflict);

            brand.Name = name;
            await db.SaveChangesAsync(ct);
            return TypedResults.Ok(new VehicleItem(brand.Id, brand.Name));
        }

        public static async Task<IResult> Delete(int id, ApplicationDbContext db, CancellationToken ct)
        {
            var brand = await db.CarBrands.FindAsync([id], ct);
            if (brand == null) return TypedResults.NotFound();
            db.CarBrands.Remove(brand);
            await db.SaveChangesAsync(ct);
            return TypedResults.NoContent();
        }

        public class Endpoints : IEndpoint
        {
            public void Map(IEndpointRouteBuilder app)
            {
                var group = app.MapGroup("/api/vehicles/brands").RequireAuthorization(Policies.Admin);
                group.MapPost("", Create).WithValidation<VehicleName>();
                group.MapPut("{id:int}", Rename).WithValidation<VehicleName>();
                group.MapDelete("{id:int}", Delete);
            }
        }
    }
}
