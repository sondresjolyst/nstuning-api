namespace nstuning_api.Services
{
    public class StorageOptions
    {
        public string ReportsPath { get; set; } = "/data/reports";
        public long MaxReportBytes { get; set; } = 52_428_800; // 50 MB
    }

    public class ReportStorageService : IReportStorageService
    {
        private const string PdfContentType = "application/pdf";
        private readonly string _root;
        private readonly long _maxBytes;
        private readonly ILogger<ReportStorageService> _logger;

        public ReportStorageService(IConfiguration configuration, ILogger<ReportStorageService> logger)
        {
            var options = configuration.GetSection("Storage").Get<StorageOptions>() ?? new StorageOptions();
            _root = Path.GetFullPath(options.ReportsPath);
            _maxBytes = options.MaxReportBytes;
            _logger = logger;
            Directory.CreateDirectory(_root);
        }

        public async Task<string> SaveAsync(IFormFile file, CancellationToken ct = default)
        {
            if (file.Length == 0)
                throw new InvalidOperationException("Uploaded file is empty.");
            if (file.Length > _maxBytes)
                throw new InvalidOperationException($"File exceeds the {_maxBytes} byte limit.");
            if (!string.Equals(file.ContentType, PdfContentType, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only PDF files are accepted.");

            var storedName = $"{Guid.NewGuid():N}.pdf";
            var fullPath = ResolvePath(storedName);

            await using (var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(stream, ct);
            }

            _logger.LogInformation("Stored report {StoredName} ({Bytes} bytes)", storedName, file.Length);
            return storedName;
        }

        public Stream OpenRead(string storedPath) =>
            new FileStream(ResolvePath(storedPath), FileMode.Open, FileAccess.Read, FileShare.Read);

        public void Delete(string storedPath)
        {
            var fullPath = ResolvePath(storedPath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        public bool Exists(string storedPath) => File.Exists(ResolvePath(storedPath));

        private string ResolvePath(string storedPath)
        {
            var name = Path.GetFileName(storedPath);
            var fullPath = Path.GetFullPath(Path.Combine(_root, name));
            if (!fullPath.StartsWith(_root, StringComparison.Ordinal))
                throw new InvalidOperationException("Invalid stored path.");
            return fullPath;
        }
    }
}
