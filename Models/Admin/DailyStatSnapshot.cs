using System.ComponentModel.DataAnnotations;

namespace nstuning_api.Models.Admin
{
    public class DailyStatSnapshot
    {
        [Key]
        public int Id { get; set; }

        public DateOnly Date { get; set; }

        public int TotalUsers { get; set; }
        public int PublishedDynoRuns { get; set; }
        public int DraftDynoRuns { get; set; }
        public int ContentImages { get; set; }
    }
}
