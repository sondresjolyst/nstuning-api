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
    public async Task CreateEngine_AddsToVariant()
    {
        await using var db = CreateDbContext();
        var (_, _, variantId) = await SeedChainAsync(db);
        Assert.IsType<Ok<VehicleItem>>(await Engines.Create(variantId, new VehicleName("2.0"), db, default));
        Assert.Equal(variantId, Assert.Single(db.CarEngines).VariantId);
    }

    [Fact]
    public async Task CreateEngine_UnknownVariant_NotFound()
    {
        await using var db = CreateDbContext();
        Assert.IsType<NotFound>(await Engines.Create(999, new VehicleName("2.0"), db, default));
    }

    [Fact]
    public async Task DeleteBrand_CascadesWholeTree()
    {
        await using var db = CreateDbContext();
        var (brandId, _, variantId) = await SeedChainAsync(db);
        db.CarEngines.Add(new CarEngine { VariantId = variantId, Name = "2.0" });
        await db.SaveChangesAsync();

        Assert.IsType<NoContent>(await Brands.Delete(brandId, db, default));
        Assert.Empty(db.CarBrands);
        Assert.Empty(db.CarModels);
        Assert.Empty(db.CarVariants);
        Assert.Empty(db.CarEngines);
    }

    [Fact]
    public async Task GetTree_ReturnsFourLevels()
    {
        await using var db = CreateDbContext();
        var (_, _, variantId) = await SeedChainAsync(db);
        db.CarEngines.Add(new CarEngine { VariantId = variantId, Name = "2.0" });
        await db.SaveChangesAsync();

        var result = await GetVehicleTree.Handle(db, default);
        var ok = Assert.IsType<Ok<List<BrandTree>>>(result);
        var brand = Assert.Single(ok.Value!);
        var model = Assert.Single(brand.Models);
        var variant = Assert.Single(model.Variants);
        Assert.Equal("2.0", Assert.Single(variant.Engines).Name);
    }
}
