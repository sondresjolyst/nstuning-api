using System.Text;
using Microsoft.AspNetCore.Http;
using nstuning_api.Services;

namespace nstuning_api.Tests;

public class FakeReportStorage : IReportStorageService
{
    public readonly Dictionary<string, byte[]> Files = new();
    public int SaveCount { get; private set; }
    public int DeleteCount { get; private set; }

    public async Task<string> SaveAsync(IFormFile file, CancellationToken ct = default)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        var name = $"{Guid.NewGuid():N}.pdf";
        Files[name] = ms.ToArray();
        SaveCount++;
        return name;
    }

    public Stream OpenRead(string storedPath) => new MemoryStream(Files[storedPath]);

    public void Delete(string storedPath)
    {
        Files.Remove(storedPath);
        DeleteCount++;
    }

    public bool Exists(string storedPath) => Files.ContainsKey(storedPath);

    public static IFormFile MakePdf(string name = "report.pdf")
    {
        var bytes = Encoding.ASCII.GetBytes("%PDF-1.4 fake");
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "Report", name)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };
    }
}
