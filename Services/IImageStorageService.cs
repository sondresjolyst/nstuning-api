namespace nstuning_api.Services
{
    public interface IImageStorageService
    {
        Task<(string StoredPath, string ContentType, long SizeBytes)> SaveAsync(IFormFile file, CancellationToken ct = default);
        Stream OpenRead(string storedPath);
        void Delete(string storedPath);
        bool Exists(string storedPath);
    }
}
