using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brain_Mint.Models
{
    [Table("Quizzes")]
    public class Quiz
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(50)]
        public string Category { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        [StringLength(20)]
        public string Difficulty { get; set; }

        [Required]
        public int TimeLimit { get; set; } // in minutes

        [Required]
        public int CreatedByUserId { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } // Active, Draft, Inactive

        // Navigation properties
        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedBy { get; set; }

        public virtual ICollection<QuizQuestion> Questions { get; set; }
    }

    [Table("QuizQuestions")]
    public class QuizQuestion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }

        [Required]
        [StringLength(500)]
        public string QuestionText { get; set; }

        [Required]
        [StringLength(200)]
        public string OptionA { get; set; }

        [Required]
        [StringLength(200)]
        public string OptionB { get; set; }

        [Required]
        [StringLength(200)]
        public string OptionC { get; set; }

        [Required]
        [StringLength(200)]
        public string OptionD { get; set; }

        [Required]
        [StringLength(1)]
        public string CorrectAnswer { get; set; } // A, B, C, or D

        [Required]
        public int QuestionOrder { get; set; }

        // Navigation property
        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; }
    }
}