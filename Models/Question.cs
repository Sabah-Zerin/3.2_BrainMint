using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Brain_Mint.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public List<string> Options { get; set; }
        public int CorrectOptionIndex { get; set; }
    }
}
