using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brain_Mint.Models
{
    [Table("QuizAttempts")]
    public class QuizAttempt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } // InProgress, Completed, Abandoned

        public int? TotalScore { get; set; }
        public int? MaximumScore { get; set; }
        public decimal? PercentageScore { get; set; }

        [StringLength(20)]
        public string GradingStatus { get; set; } // AutoGraded, PendingReview, Reviewed

        // Navigation properties
        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; }

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; }

        public virtual ICollection<QuizResponse> Responses { get; set; }
    }
}