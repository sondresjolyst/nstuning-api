using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using nstuning_api.Services;
using Xunit;

namespace nstuning_api.Tests;

public class ReportStorageServiceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "nstuning-tests-" + Guid.NewGuid().ToString("N"));
    private readonly ReportStorageService _service;

    public ReportStorageServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:ReportsPath"] = _dir,
                ["Storage:MaxReportBytes"] = "1048576"
            })
            .Build();
        _service = new ReportStorageService(config, NullLogger<ReportStorageService>.Instance);
    }

    private static IFormFile MakeFile(string contentType, byte[]? bytes = null, string name = "file.pdf")
    {
        bytes ??= Encoding.ASCII.GetBytes("%PDF-1.4 data");
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "Report", name)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    [Fact]
    public async Task SaveAsync_Pdf_StoresAndCanReadBack()
    {
        var stored = await _service.SaveAsync(MakeFile("application/pdf"));
        Assert.True(_service.Exists(stored));

        using var reader = new StreamReader(_service.OpenRead(stored));
        Assert.StartsWith("%PDF", await reader.ReadToEndAsync());
    }

    [Fact]
    public async Task SaveAsync_NonPdf_Throws()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SaveAsync(MakeFile("image/png")));
    }

    [Fact]
    public async Task SaveAsync_OverSize_Throws()
    {
        var big = new byte[2 * 1024 * 1024];
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.SaveAsync(MakeFile("application/pdf", big)));
    }

    [Fact]
    public async Task Delete_RemovesFile()
    {
        var stored = await _service.SaveAsync(MakeFile("application/pdf"));
        _service.Delete(stored);
        Assert.False(_service.Exists(stored));
    }

    [Fact]
    public void OpenRead_PathTraversal_IsContained()
    {
        Assert.Throws<FileNotFoundException>(() => _service.OpenRead("../../etc/passwd"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
            Directory.Delete(_dir, true);
    }
}
