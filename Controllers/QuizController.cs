using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Brain_Mint.Models;
using System.Data.Entity;

namespace Brain_Mint.Controllers
{
    public class QuizController : Controller
    {
        private BrainMintDbContext db = new BrainMintDbContext();

        // For students to take quizzes
        public ActionResult Index()
        {
            if (Session["UserName"] == null)
                return RedirectToAction("Login", "Account");

            // Get active quizzes for students
            var activeQuizzes = db.Quizzes
                .Where(q => q.Status == "Active")
                .Include(q => q.Questions)
                .ToList();

            return View(activeQuizzes);
        }

        // Take a specific quiz
        public ActionResult Take(int id)
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            var quiz = db.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefault(q => q.Id == id && q.Status == "Active");

            if (quiz == null)
            {
                TempData["Error"] = "Quiz not found or not available.";
                return RedirectToAction("Index");
            }

            return View(quiz);
        }

        [HttpPost]
        public ActionResult Submit(int quizId, Dictionary<int, string> answers)
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            var quiz = db.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefault(q => q.Id == quizId && q.Status == "Active");

            if (quiz == null)
            {
                TempData["Error"] = "Quiz not found.";
                return RedirectToAction("Index");
            }

            int score = 0;
            int totalQuestions = quiz.Questions.Count();

            foreach (var question in quiz.Questions)
            {
                if (answers.ContainsKey(question.Id))
                {
                    string userAnswer = answers[question.Id];

                    if (question.QuestionType == "MultipleChoice")
                    {
                        if (userAnswer.ToUpper() == question.CorrectAnswer.ToUpper())
                        {
                            score++;
                        }
                    }
                    // For short answer questions, you might want to implement more sophisticated checking
                    // For now, we'll just check if they provided an answer
                    else if (!string.IsNullOrEmpty(userAnswer))
                    {
                        // This is a placeholder - you might want manual grading for short answers
                        score++; // or implement keyword matching logic
                    }
                }
            }

            ViewBag.Score = score;
            ViewBag.Total = totalQuestions;
            ViewBag.Percentage = totalQuestions > 0 ? (score * 100) / totalQuestions : 0;
            ViewBag.QuizTitle = quiz.Title;

            return View("Result");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}