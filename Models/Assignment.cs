using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Brain_Mint.Models
{
    // Assignment Entity
    [Table("Assignments")]
    public class Assignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        public string Subject { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public int CreatedByUserId { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } // Active, Inactive, Draft

        [Required]
        public int MaxPoints { get; set; }

        [StringLength(20)]
        public string SubmissionType { get; set; } = "Both"; // Text, Image, Both

        // Navigation properties
        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedBy { get; set; }

        public virtual ICollection<AssignmentSubmission> Submissions { get; set; }
    }

    // Assignment Submission Entity
    [Table("AssignmentSubmissions")]
    public class AssignmentSubmission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        [Required]
        public int StudentId { get; set; }

        public string TextSubmission { get; set; }

        [StringLength(500)]
        public string ImagePath { get; set; }

        [StringLength(200)]
        public string OriginalFileName { get; set; }

        [Required]
        public DateTime SubmissionDate { get; set; }

        public DateTime? LastModified { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Submitted"; // Submitted, Late, Graded

        public int? Points { get; set; }

        public string TeacherFeedback { get; set; }

        public DateTime? GradedDate { get; set; }

        public int? GradedByUserId { get; set; }

        // Navigation properties
        [ForeignKey("AssignmentId")]
        public virtual Assignment Assignment { get; set; }

        [ForeignKey("StudentId")]
        public virtual User Student { get; set; }

        [ForeignKey("GradedByUserId")]
        public virtual User GradedBy { get; set; }
    }

    // View Models
    public class AssignmentStudentViewModel
    {
        public Assignment Assignment { get; set; }
        public bool HasSubmitted { get; set; }
        public AssignmentSubmission Submission { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysUntilDue { get; set; }
    }

    public class SubmitAssignmentViewModel
    {
        public int AssignmentId { get; set; }
        public Assignment Assignment { get; set; }
        public bool HasExistingSubmission { get; set; }
        public AssignmentSubmission ExistingSubmission { get; set; }
        public string TextSubmission { get; set; }
    }

    public class CreateAssignmentViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        public string Subject { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [Range(1, 1000)]
        public int MaxPoints { get; set; }

        [Required]
        public string SubmissionType { get; set; } = "Both";
    }

    public class AssignmentDetailsViewModel
    {
        public Assignment Assignment { get; set; }
        public List<AssignmentSubmissionView> Submissions { get; set; }
        public int TotalSubmissions { get; set; }
        public int PendingGrading { get; set; }
        public double AverageScore { get; set; }
    }

    public class AssignmentSubmissionView
    {
        public int Id { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string Status { get; set; }
        public int? Points { get; set; }
        public bool HasTextSubmission { get; set; }
        public bool HasImageSubmission { get; set; }
        public bool IsLate { get; set; }
    }
}