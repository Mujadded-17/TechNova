namespace TechNova.Models
{
    public class PitchDeck
    {
        public int PitchID { get; set; }

        public int StartupID { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        // Relationship
        public Startup Startup { get; set; } = null!;
    }
}