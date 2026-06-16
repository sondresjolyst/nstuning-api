using SkiaSharp;

namespace nstuning_api.Services
{
    public class ImageStorageService : FileStorageBase, IImageStorageService
    {
        // Target widths for responsive srcset. Only widths smaller than the source are
        // resized; a full-size variant is always added (capped at MaxWebpWidth).
        private static readonly int[] TargetWidths = [384, 640, 768, 1024, 1366];
        private const int MaxWebpWidth = 2000;
        private const int WebpQuality = 80;

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

        public async Task<IReadOnlyList<(int Width, string StoredPath, long SizeBytes)>> GenerateWebpVariantsAsync(string originalStoredPath, CancellationToken ct = default)
        {
            try
            {
                using var input = OpenRead(originalStoredPath);
                using var original = SKBitmap.Decode(input);
                if (original == null || original.Width == 0)
                {
                    _logger.LogWarning("Could not decode image {StoredPath} for webp generation", originalStoredPath);
                    return [];
                }

                var fullWidth = Math.Min(original.Width, MaxWebpWidth);
                var widths = TargetWidths
                    .Where(w => w < fullWidth)
                    .Append(fullWidth)
                    .Distinct()
                    .OrderBy(w => w)
                    .ToList();

                var results = new List<(int Width, string StoredPath, long SizeBytes)>();
                foreach (var width in widths)
                {
                    ct.ThrowIfCancellationRequested();

                    var height = (int)Math.Round((double)original.Height * width / original.Width);
                    var target = width == original.Width
                        ? original
                        : original.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
                    if (target == null) continue;

                    try
                    {
                        using var image = SKImage.FromBitmap(target);
                        using var data = image.Encode(SKEncodedImageFormat.Webp, WebpQuality);
                        if (data == null) continue;

                        var bytes = data.ToArray();
                        var storedName = $"{Guid.NewGuid():N}.webp";
                        await WriteBytesAsync(storedName, bytes, ct);
                        results.Add((width, storedName, bytes.Length));
                    }
                    finally
                    {
                        if (!ReferenceEquals(target, original)) target.Dispose();
                    }
                }

                _logger.LogInformation("Generated {Count} webp variants for {StoredPath}", results.Count, originalStoredPath);
                return results;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed generating webp variants for {StoredPath}", originalStoredPath);
                return [];
            }
        }
    }
}
