namespace nstuning_api.Services
{
    public class StorageOptions
    {
        public string ReportsPath { get; set; } = "/data/reports";
        public long MaxReportBytes { get; set; } = 52_428_800; // 50 MB
        public string ImagesPath { get; set; } = "/data/images";
        public long MaxImageBytes { get; set; } = 5_242_880; // 5 MB
    }

    public class ReportStorageService : FileStorageBase, IReportStorageService
    {
        private const string PdfContentType = "application/pdf";
        private readonly ILogger<ReportStorageService> _logger;

        public ReportStorageService(IConfiguration configuration, ILogger<ReportStorageService> logger)
            : this(LoadOptions(configuration), logger) { }

        private ReportStorageService(StorageOptions options, ILogger<ReportStorageService> logger)
            : base(options.ReportsPath, options.MaxReportBytes)
        {
            _logger = logger;
        }

        private static StorageOptions LoadOptions(IConfiguration configuration) =>
            configuration.GetSection("Storage").Get<StorageOptions>() ?? new StorageOptions();

        public async Task<string> SaveAsync(IFormFile file, CancellationToken ct = default)
        {
            if (file.Length == 0)
                throw new AppValidationException("Uploaded file is empty.");
            if (file.Length > MaxBytes)
                throw new AppValidationException($"File exceeds the {MaxBytes / 1_048_576} MB limit.");
            if (!string.Equals(file.ContentType, PdfContentType, StringComparison.OrdinalIgnoreCase))
                throw new AppValidationException("Only PDF files are accepted.");

            var storedName = $"{Guid.NewGuid():N}.pdf";
            await WriteAsync(storedName, file, ct);

            _logger.LogInformation("Stored report {StoredName} ({Bytes} bytes)", storedName, file.Length);
            return storedName;
        }
    }
}
