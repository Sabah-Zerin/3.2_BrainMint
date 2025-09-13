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

        [StringLength(1000)]
        public string StudentAnswer { get; set; }

        public bool? IsCorrect { get; set; } // null for ungraded short answers

        public int? PointsAwarded { get; set; }

        [StringLength(500)]
        public string TeacherFeedback { get; set; }

        public DateTime ResponseTime { get; set; }

        // Navigation properties
        [ForeignKey("QuizAttemptId")]
        public virtual QuizAttempt QuizAttempt { get; set; }

        [ForeignKey("QuestionId")]
        public virtual QuizQuestion Question { get; set; }
    }
}