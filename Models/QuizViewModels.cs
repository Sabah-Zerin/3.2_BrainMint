using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Brain_Mint.Models
{
    public class TakeQuizViewModel
    {
        public Quiz Quiz { get; set; }
        public int AttemptId { get; set; }
        public List<QuizQuestion> Questions { get; set; }
        public DateTime StartTime { get; set; }
    }

    public class QuizViewModel
    {
        public Quiz Quiz { get; set; }
        public bool HasAttempted { get; set; }
        public QuizAttempt LastAttempt { get; set; }
    }

    public class ScoreRecord
    {
        public string Name { get; set; }
        public int Score { get; set; }
    }
}