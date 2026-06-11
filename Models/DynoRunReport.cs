using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nstuning_api.Models
{
    public class DynoRunReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DynoRunId { get; set; }

        [ForeignKey(nameof(DynoRunId))]
        public DynoRun? DynoRun { get; set; }

        [Required]
        [MaxLength(260)]
        public required string FileName { get; set; }

        [Required]
        [MaxLength(100)]
        public required string ContentType { get; set; }

        public long SizeBytes { get; set; }

        [Required]
        [MaxLength(400)]
        public required string StoredPath { get; set; }

        [MaxLength(450)]
        public string? UploadedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
