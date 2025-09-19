// QuizParticipationViewModel.cs

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Brain_Mint.Models
{
    public class QuizParticipationViewModel
    {
        public List<QuizParticipationInfo> ActiveQuizzes { get; set; } = new List<QuizParticipationInfo>();
        public QuizQuickStats QuickStats { get; set; }
        public int TotalQuizzes { get; set; }
        public int TotalParticipants { get; set; }
    }

    public class QuizParticipationInfo
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int ParticipantCount { get; set; }
        public decimal? AverageScore { get; set; }
        public DateTime CreatedDate { get; set; }
        public int TotalQuestions { get; set; }
        public int TimeLimit { get; set; }
        public int CompletedAttempts { get; set; }
        public string TeacherName { get; set; }
    }

    public class QuizQuickStats
    {
        public int TotalQuizzesCreated { get; set; }
        public int TotalAttempts { get; set; }
        public decimal OverallAverageScore { get; set; }
        public int ActiveQuizzesCount { get; set; }
        public int TotalUniqueParticipants { get; set; }
    }
}