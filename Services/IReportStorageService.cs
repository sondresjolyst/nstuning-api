namespace nstuning_api.Services
{
    public interface IReportStorageService
    {
        Task<string> SaveAsync(IFormFile file, CancellationToken ct = default);
        Stream OpenRead(string storedPath);
        void Delete(string storedPath);
        bool Exists(string storedPath);
    }
}
