using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Brain_Mint.Models;
namespace Brain_Mint.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private static List<Question> questions = new List<Question>
        {
            new Question
            {
                Id = 1,
                Text = "What is the capital of France?",
                Options = new List<string> { "Paris", "London", "Berlin", "Madrid" },
                CorrectOptionIndex = 0
            },
            new Question
            {
                Id = 2,
                Text = "Which planet is known as the Red Planet?",
                Options = new List<string> { "Earth", "Mars", "Jupiter", "Venus" },
                CorrectOptionIndex = 1
            }
        };

        [Authorize]
        public ActionResult Index()
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Account");

            return View(questions);
        }


        [HttpPost]
        public ActionResult Submit(int[] answers)
        {
            int score = 0;
            for (int i = 0; i < questions.Count; i++)
            {
                if (answers[i] == questions[i].CorrectOptionIndex)
                    score++;
            }
            ViewBag.Score = score;
            ViewBag.Total = questions.Count;
            return View("Result");
        }
    }
}
