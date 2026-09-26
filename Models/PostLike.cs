using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechNova.Models
{
    public class PostLike
    {
        public int PostLikeID { get; set; }

        [Required]
        public int PostID { get; set; }

        [Required]
        public int InvestorID { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relationships
        [ForeignKey("PostID")]
        public Post? Post { get; set; }

        [ForeignKey("InvestorID")]
        public Investor? Investor { get; set; }
    }
}
