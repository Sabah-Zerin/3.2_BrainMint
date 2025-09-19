//  Quiz.cs MODEL
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

        // ADD THIS - Missing navigation property
        public virtual ICollection<QuizAttempt> QuizAttempts { get; set; }
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

        // Make these optional for short answer questions
        [StringLength(200)]
        public string OptionA { get; set; }

        [StringLength(200)]
        public string OptionB { get; set; }

        [StringLength(200)]
        public string OptionC { get; set; }

        [StringLength(200)]
        public string OptionD { get; set; }

        // This can be A,B,C,D for multiple choice or text for short answer
        [StringLength(1000)]
        public string CorrectAnswer { get; set; }

        [Required]
        public int QuestionOrder { get; set; }

        // Add question type field
        [StringLength(20)]
        public string QuestionType { get; set; } = "MultipleChoice"; // MultipleChoice or ShortAnswer

        // ADD THIS - Missing MaxPoints property
        [Required]
        public int MaxPoints { get; set; } = 1;

        // Navigation property
        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; }
    }
}