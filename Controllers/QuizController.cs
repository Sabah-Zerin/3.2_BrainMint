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
