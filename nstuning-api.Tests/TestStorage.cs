using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nstuning_api.Models;
using nstuning_api.Services;
using SjolystInnovation.AppKit.Storage;
using SjolystInnovation.AppKit.Storage.Images;
using SkiaSharp;

namespace nstuning_api.Tests;

/// <summary>The app's real report store and content image service, on a temporary directory.</summary>
public sealed class TestStorage : IDisposable
{
    private readonly ServiceProvider _services;
    private readonly IServiceScope _scope;

    public TestStorage(ApplicationDbContext db)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:ReportsPath"] = ReportsPath,
                ["Storage:ImagesPath"] = ImagesPath,
            })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(db);
        services.AddReportStore(config);
        services.AddAppKitContentImages<ApplicationDbContext>(config.GetSection("Storage"));
        _services = services.BuildServiceProvider();
        _scope = _services.CreateScope();
    }

    public string Root { get; } = Path.Combine(Path.GetTempPath(), "nstuning-tests-" + Guid.NewGuid().ToString("N"));
    public string ReportsPath => Path.Combine(Root, "reports");
    public string ImagesPath => Path.Combine(Root, "images");

    public IServiceProvider Services => _scope.ServiceProvider;
    public IAppKitFileStore Reports => _services.GetRequiredKeyedService<IAppKitFileStore>(ReportStore.Name);
    public IContentImageService Images => _scope.ServiceProvider.GetRequiredService<IContentImageService>();

    public string[] ReportFiles => Directory.Exists(ReportsPath) ? Directory.GetFiles(ReportsPath) : [];
    public string[] ImageFiles => Directory.Exists(ImagesPath) ? Directory.GetFiles(ImagesPath) : [];

    public static IFormFile MakePdf(string name = "report.pdf") =>
        MakeFile(Encoding.ASCII.GetBytes("%PDF-1.4 fake"), name, "application/pdf", "Report");

    public static IFormFile MakePng(string name = "photo.png", int width = 20, int height = 10)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.Goldenrod);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return MakeFile(data.ToArray(), name, "image/png", "CoverImage");
    }

    public static IFormFile MakeFile(byte[] bytes, string name, string contentType, string field = "file") =>
        new FormFile(new MemoryStream(bytes), 0, bytes.Length, field, name)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };

    public void Dispose()
    {
        _scope.Dispose();
        _services.Dispose();
        if (Directory.Exists(Root))
            Directory.Delete(Root, true);
    }
}
