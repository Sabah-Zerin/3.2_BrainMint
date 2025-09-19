using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Brain_Mint.Models
{
    public class AssignmentParticipationViewModel
    {
        public List<AssignmentParticipationInfo> ActiveAssignments { get; set; } = new List<AssignmentParticipationInfo>();
        public AssignmentQuickStats QuickStats { get; set; }
        public int TotalAssignments { get; set; }
        public int TotalParticipants { get; set; }
    }

    public class AssignmentParticipationInfo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Subject { get; set; }
        public DateTime DueDate { get; set; }
        public int MaxPoints { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; }
        public int ParticipantCount { get; set; }
        public int TotalSubmissions { get; set; }
        public int GradedSubmissions { get; set; }
        public decimal? AverageScore { get; set; }

        // Added property for teacher name
        public string TeacherName { get; set; }
    }

    public class AssignmentQuickStats
    {
        public int TotalAssignmentsCreated { get; set; }
        public int TotalSubmissions { get; set; }
        public decimal OverallAverageScore { get; set; }
        public int ActiveAssignmentsCount { get; set; }
        public int TotalUniqueParticipants { get; set; }
    }
}
