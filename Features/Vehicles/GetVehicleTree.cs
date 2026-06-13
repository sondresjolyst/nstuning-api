using Microsoft.EntityFrameworkCore;
using nstuning_api.Constants;
using nstuning_api.Infrastructure;
using nstuning_api.Models;

namespace nstuning_api.Features.Vehicles
{
    /// <summary>GET the full brand → model → variant → engine tree.</summary>
    public static class GetVehicleTree
    {
        public static async Task<IResult> Handle(ApplicationDbContext db, CancellationToken ct)
        {
            var brands = await db.CarBrands.OrderBy(b => b.Name).ToListAsync(ct);
            var models = await db.CarModels.OrderBy(m => m.Name).ToListAsync(ct);
            var variants = await db.CarVariants.OrderBy(v => v.Name).ToListAsync(ct);
            var engines = await db.CarEngines.OrderBy(e => e.Name).ToListAsync(ct);

            var tree = brands.Select(b => new BrandTree(b.Id, b.Name,
                models.Where(m => m.BrandId == b.Id).Select(m => new ModelTree(m.Id, m.Name,
                    variants.Where(v => v.ModelId == m.Id).Select(v => new VariantTree(v.Id, v.Name,
                        engines.Where(e => e.VariantId == v.Id).Select(e => new VehicleItem(e.Id, e.Name)).ToList()
                    )).ToList()
                )).ToList()
            )).ToList();

            return TypedResults.Ok(tree);
        }

        public class Endpoint : IEndpoint
        {
            public void Map(IEndpointRouteBuilder app) =>
                app.MapGet("/api/vehicles/tree", Handle).RequireAuthorization(Policies.Admin);
        }
    }
}
