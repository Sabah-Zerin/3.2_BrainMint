using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Brain_Mint.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
    }

    public class StudentPerformance
    {
        public List<ScoreRecord> QuizScores { get; set; }
        public List<ScoreRecord> AssignmentScores { get; set; }
    }

    public class ScoreRecord
    {
        public string Name { get; set; }
        public int Score { get; set; }
    }
}