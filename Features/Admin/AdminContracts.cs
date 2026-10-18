namespace nstuning_api.Features.Admin
{
    public class AdminStatsDto
    {
        public int TotalUsers { get; set; }
        public int PublishedDynoRuns { get; set; }
        public int DraftDynoRuns { get; set; }
        public int ContentImages { get; set; }
        public long StorageUsedBytes { get; set; }
        public long DiskTotalBytes { get; set; }
        public long DiskFreeBytes { get; set; }
    }
}
