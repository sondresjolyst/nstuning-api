using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using nstuning_api.Features.Vehicles;
using nstuning_api.Models;
using Xunit;

namespace nstuning_api.Tests;

public class VehicleSlicesTests : TestBase
{
    private static async Task<(int brandId, int modelId, int variantId)> SeedChainAsync(ApplicationDbContext db)
    {
        var brand = new CarBrand { Name = "Saab" };
        db.CarBrands.Add(brand);
        await db.SaveChangesAsync();
        var model = new CarModel { BrandId = brand.Id, Name = "99" };
        db.CarModels.Add(model);
        await db.SaveChangesAsync();
        var variant = new CarVariant { ModelId = model.Id, Name = "EMS" };
        db.CarVariants.Add(variant);
        await db.SaveChangesAsync();
        return (brand.Id, model.Id, variant.Id);
    }

    [Fact]
    public async Task CreateBrand_AddsTrimmed()
    {
        await using var db = CreateDbContext();
        Assert.IsType<Ok<VehicleItem>>(await Brands.Create(new VehicleName(" Saab "), db, default));
        Assert.Equal("Saab", Assert.Single(db.CarBrands).Name);
    }

    [Fact]
    public async Task CreateBrand_DuplicateCaseInsensitive_NoDuplicate()
    {
        await using var db = CreateDbContext();
        db.CarBrands.Add(new CarBrand { Name = "Saab" });
        await db.SaveChangesAsync();
        await Brands.Create(new VehicleName("saab"), db, default);
        Assert.Single(db.CarBrands);
    }

    [Fact]
    public async Task RenameBrand_DuplicateName_Conflict()
    {
        await using var db = CreateDbContext();
        db.CarBrands.AddRange(new CarBrand { Name = "Saab" }, new CarBrand { Name = "Volvo" });
        await db.SaveChangesAsync();
        var volvo = db.CarBrands.Single(b => b.Name == "Volvo");

        var result = await Brands.Rename(volvo.Id, new VehicleName("saab"), db, default);
        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status409Conflict, problem.StatusCode);
    }

    [Fact]
    public async Task CreateModel_UnknownBrand_NotFound()
    {
        await using var db = CreateDbContext();
        Assert.IsType<NotFound>(await CarModels.Create(999, new VehicleName("99"), db, default));
    }

    [Fact]
    public async Task SetFamily_SetsAndClears()
    {
        await using var db = CreateDbContext();
        var (_, modelId, _) = await SeedChainAsync(db);

        Assert.IsType<Ok<VehicleItem>>(await CarModels.SetFamily(modelId, new ModelFamily(" 240 series "), db, default));
        Assert.Equal("240 series", db.CarModels.Single(m => m.Id == modelId).Family);

        await CarModels.SetFamily(modelId, new ModelFamily(""), db, default);
        Assert.Null(db.CarModels.Single(m => m.Id == modelId).Family);
    }

    [Fact]
    public async Task CreateEngine_AddsToCatalogWithBrand()
    {
        await using var db = CreateDbContext();
        var (brandId, _, _) = await SeedChainAsync(db);
        Assert.IsType<Ok<EngineCatalogItem>>(await Engines.Create(new EngineInput("2.0", brandId), db, default));
        var engine = Assert.Single(db.CarEngines);
        Assert.Equal(brandId, engine.BrandId);
        Assert.Equal("2.0", engine.Name);
    }

    [Fact]
    public async Task CreateEngine_UnknownBrand_NotFound()
    {
        await using var db = CreateDbContext();
        Assert.IsType<NotFound>(await Engines.Create(new EngineInput("2.0", 999), db, default));
    }

    [Fact]
    public async Task CreateEngine_GlobalNoBrand_Allowed()
    {
        await using var db = CreateDbContext();
        Assert.IsType<Ok<EngineCatalogItem>>(await Engines.Create(new EngineInput("2JZ-GTE", null), db, default));
        Assert.Null(Assert.Single(db.CarEngines).BrandId);
    }

    [Fact]
    public async Task LinkAndUnlinkEngine_ManagesFitment()
    {
        await using var db = CreateDbContext();
        var (brandId, modelId, _) = await SeedChainAsync(db);
        await Engines.Create(new EngineInput("2.0", brandId), db, default);
        var engineId = db.CarEngines.Single().Id;

        Assert.IsType<NoContent>(await Engines.Link(modelId, engineId, db, default));
        Assert.Single(db.CarModelEngines);
        // idempotent
        await Engines.Link(modelId, engineId, db, default);
        Assert.Single(db.CarModelEngines);

        Assert.IsType<NoContent>(await Engines.Unlink(modelId, engineId, db, default));
        Assert.Empty(db.CarModelEngines);
    }

    [Fact]
    public async Task LinkEngine_UnknownModel_NotFound()
    {
        await using var db = CreateDbContext();
        var (brandId, _, _) = await SeedChainAsync(db);
        await Engines.Create(new EngineInput("2.0", brandId), db, default);
        var engineId = db.CarEngines.Single().Id;
        Assert.IsType<NotFound>(await Engines.Link(999, engineId, db, default));
    }

    [Fact]
    public async Task DeleteBrand_CascadesModelsAndVariants()
    {
        await using var db = CreateDbContext();
        var (brandId, _, _) = await SeedChainAsync(db);

        Assert.IsType<NoContent>(await Brands.Delete(brandId, db, default));
        Assert.Empty(db.CarBrands);
        Assert.Empty(db.CarModels);
        Assert.Empty(db.CarVariants);
    }

    [Fact]
    public async Task GetTree_ReturnsBrandsVariantsAndEngineCatalog()
    {
        await using var db = CreateDbContext();
        var (brandId, modelId, _) = await SeedChainAsync(db);
        await Engines.Create(new EngineInput("2.0", brandId), db, default);
        var engineId = db.CarEngines.Single().Id;
        await Engines.Link(modelId, engineId, db, default);

        var result = await GetVehicleTree.Handle(db, default);
        var ok = Assert.IsType<Ok<VehicleTreeResponse>>(result);
        var brand = Assert.Single(ok.Value!.Brands);
        var model = Assert.Single(brand.Models);
        Assert.Equal("EMS", Assert.Single(model.Variants).Name);
        Assert.Equal(engineId, Assert.Single(model.EngineIds));
        Assert.Equal("2.0", Assert.Single(ok.Value!.Engines).Name);
    }
}
