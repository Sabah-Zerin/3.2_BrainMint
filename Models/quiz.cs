using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Brainmint.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<Question> Questions { get; set; } = new List<Question>();
    }

    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public List<Answer> Answers { get; set; } = new List<Answer>();
        public int CorrectAnswerId { get; set; }
    }

    public class Answer
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }
}