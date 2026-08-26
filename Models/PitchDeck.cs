using System.ComponentModel.DataAnnotations;

namespace TechNova.Models
{
    public class PitchDeck
    {
        public int PitchID { get; set; }

        public int StartupID { get; set; }

        /// <summary>Original name as uploaded, shown to users. Never used on disk.</summary>
        [MaxLength(260)]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Generated name on disk, inside App_Data/pitch-decks.
        /// Stored outside wwwroot so uploads are never directly servable —
        /// every download goes through an access-controlled action.
        /// </summary>
        [MaxLength(120)]
        public string FilePath { get; set; } = string.Empty;

        /// <summary>Label the founder gives it, e.g. "Seed round deck".</summary>
        [MaxLength(120)]
        public string? Title { get; set; }

        [MaxLength(120)]
        public string ContentType { get; set; } = "application/pdf";

        public long FileSizeBytes { get; set; }

        /// <summary>Only one deck is shown on the public profile.</summary>
        public bool IsPrimary { get; set; } = false;

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        // Relationship
        public Startup Startup { get; set; } = null!;
    }
}
