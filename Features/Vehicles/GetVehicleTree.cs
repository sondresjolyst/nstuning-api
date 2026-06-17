using Microsoft.EntityFrameworkCore;
using nstuning_api.Constants;
using nstuning_api.Infrastructure;
using nstuning_api.Models;

namespace nstuning_api.Features.Vehicles
{
    /// <summary>GET the brand → model → variant tree plus the flat engine catalog and factory-fitment links.</summary>
    public static class GetVehicleTree
    {
        public static async Task<IResult> Handle(ApplicationDbContext db, CancellationToken ct)
        {
            var brands = await db.CarBrands.OrderBy(b => b.Name).ToListAsync(ct);
            var models = await db.CarModels.OrderBy(m => m.Name).ToListAsync(ct);
            var variants = await db.CarVariants.OrderBy(v => v.Name).ToListAsync(ct);
            var engines = await db.CarEngines.OrderBy(e => e.Name).ToListAsync(ct);
            var fitments = await db.CarModelEngines.ToListAsync(ct);

            var brandTree = brands.Select(b => new BrandTree(b.Id, b.Name,
                models.Where(m => m.BrandId == b.Id).Select(m => new ModelTree(
                    m.Id, m.Name, m.Family,
                    variants.Where(v => v.ModelId == m.Id).Select(v => new VariantItem(v.Id, v.Name)).ToList(),
                    fitments.Where(f => f.ModelId == m.Id).Select(f => f.EngineId).ToList()
                )).ToList()
            )).ToList();

            var catalog = engines.Select(e => new EngineCatalogItem(e.Id, e.Name, e.BrandId)).ToList();

            return TypedResults.Ok(new VehicleTreeResponse(brandTree, catalog));
        }

        public class Endpoint : IEndpoint
        {
            public void Map(IEndpointRouteBuilder app) =>
                app.MapGet("/api/vehicles/tree", Handle).RequireAuthorization(Policies.Admin);
        }
    }
}
