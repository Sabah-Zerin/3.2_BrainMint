using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Brain_Mint.Models
{
    public class QuizStatistics
    {
        public int TotalAttempts { get; set; }
        public int GradedAttempts { get; set; }
        public decimal AverageScore { get; set; }
        public decimal HighestScore { get; set; }
        public decimal LowestScore { get; set; }
        public int TotalPossiblePoints { get; set; }
        public decimal AveragePointsEarned { get; set; }
    }
}