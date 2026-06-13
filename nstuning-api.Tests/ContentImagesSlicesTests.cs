using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using nstuning_api.Features.ContentImages;
using nstuning_api.Models;
using Xunit;

namespace nstuning_api.Tests;

public class ContentImagesSlicesTests : TestBase
{
    [Fact]
    public async Task Upload_StoresFileAndRow()
    {
        await using var db = CreateDbContext();
        var storage = new FakeImageStorage();

        var result = await ContentImages.Upload(FakeImageStorage.MakeImage(), new DefaultHttpContext(), db, storage, default);

        Assert.IsType<Ok<UploadedImage>>(result);
        Assert.Equal(1, storage.SaveCount);
        Assert.Single(db.ContentImages);
    }

    [Fact]
    public async Task Get_Existing_ReturnsFile()
    {
        await using var db = CreateDbContext();
        var storage = new FakeImageStorage();
        storage.Files["x.png"] = [1, 2, 3];
        db.ContentImages.Add(new ContentImage { FileName = "x.png", ContentType = "image/png", StoredPath = "x.png" });
        await db.SaveChangesAsync();
        var image = db.ContentImages.First();

        var result = await ContentImages.Get(image.Id, new DefaultHttpContext(), db, storage, default);
        Assert.IsType<FileStreamHttpResult>(result);
    }

    [Fact]
    public async Task Get_Missing_ReturnsNotFound()
    {
        await using var db = CreateDbContext();
        var result = await ContentImages.Get("missing", new DefaultHttpContext(), db, new FakeImageStorage(), default);
        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task Delete_RemovesFileAndRow()
    {
        await using var db = CreateDbContext();
        var storage = new FakeImageStorage();
        storage.Files["x.png"] = [1];
        db.ContentImages.Add(new ContentImage { FileName = "x.png", ContentType = "image/png", StoredPath = "x.png" });
        await db.SaveChangesAsync();
        var image = db.ContentImages.First();

        var result = await ContentImages.Delete(image.Id, db, storage, default);
        Assert.IsType<NoContent>(result);
        Assert.Equal(1, storage.DeleteCount);
        Assert.Empty(db.ContentImages);
    }
}
