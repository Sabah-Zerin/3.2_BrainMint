using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brain_Mint.Models
{
    [Table("QuizResponses")]
    public class QuizResponse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int QuizAttemptId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        public string StudentAnswer { get; set; }

        public bool? IsCorrect { get; set; }

        public int? PointsAwarded { get; set; }

        public string TeacherFeedback { get; set; }

        public DateTime ResponseTime { get; set; }

        // Add these new properties
        public DateTime? GradedDate { get; set; }

        public int? GradedByUserId { get; set; }

        // Navigation properties
        [ForeignKey("QuizAttemptId")]
        public virtual QuizAttempt QuizAttempt { get; set; }

        [ForeignKey("QuestionId")]
        public virtual QuizQuestion Question { get; set; }

        [ForeignKey("GradedByUserId")]
        public virtual User GradedBy { get; set; }
    }
}