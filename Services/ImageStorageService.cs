namespace nstuning_api.Services
{
    public class ImageStorageService : FileStorageBase, IImageStorageService
    {
        private static readonly Dictionary<string, string> Extensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp",
            ["image/gif"] = ".gif",
            ["image/avif"] = ".avif",
        };

        private readonly ILogger<ImageStorageService> _logger;

        public ImageStorageService(IConfiguration configuration, ILogger<ImageStorageService> logger)
            : this(LoadOptions(configuration), logger) { }

        private ImageStorageService(StorageOptions options, ILogger<ImageStorageService> logger)
            : base(options.ImagesPath, options.MaxImageBytes)
        {
            _logger = logger;
        }

        private static StorageOptions LoadOptions(IConfiguration configuration) =>
            configuration.GetSection("Storage").Get<StorageOptions>() ?? new StorageOptions();

        public async Task<(string StoredPath, string ContentType, long SizeBytes)> SaveAsync(IFormFile file, CancellationToken ct = default)
        {
            if (file.Length == 0)
                throw new AppValidationException("Uploaded file is empty.");
            if (file.Length > MaxBytes)
                throw new AppValidationException($"Image exceeds the {MaxBytes / 1_048_576} MB limit.");
            if (!Extensions.TryGetValue(file.ContentType, out var extension))
                throw new AppValidationException("Unsupported image type.");

            var storedName = $"{Guid.NewGuid():N}{extension}";
            await WriteAsync(storedName, file, ct);

            _logger.LogInformation("Stored image {StoredName} ({Bytes} bytes)", storedName, file.Length);
            return (storedName, file.ContentType, file.Length);
        }
    }
}
