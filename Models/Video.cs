using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechNova.Models
{
    public class Video
    {
        public int VideoID { get; set; }

        [Required]
        public int StartupID { get; set; }

        public int? PostID { get; set; }

        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? MimeType { get; set; }

        public long FileSizeBytes { get; set; }

        public int? DurationSeconds { get; set; }

        public string? ThumbnailPath { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        // Relationships
        [ForeignKey("StartupID")]
        public Startup? Startup { get; set; }

        [ForeignKey("PostID")]
        public Post? Post { get; set; }
    }
}
