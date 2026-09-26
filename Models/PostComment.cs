using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechNova.Models
{
    public class PostComment
    {
        public int PostCommentID { get; set; }

        [Required]
        public int PostID { get; set; }

        [Required]
        public int InvestorID { get; set; }

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        // Relationships
        [ForeignKey("PostID")]
        public Post? Post { get; set; }

        [ForeignKey("InvestorID")]
        public Investor? Investor { get; set; }
    }
}
