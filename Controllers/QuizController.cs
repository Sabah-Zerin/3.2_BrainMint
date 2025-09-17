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

        public ActionResult Index()
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            int studentId = Convert.ToInt32(Session["UserId"]);

            // Get active quizzes with attempt information
            var activeQuizzes = db.Quizzes
                .Where(q => q.Status == "Active")
                .Include(q => q.Questions)
                .ToList()
                .Select(q => new QuizViewModel
                {
                    Quiz = q,
                    HasAttempted = db.QuizAttempts.Any(qa => qa.QuizId == q.Id && qa.StudentId == studentId && qa.Status == "Completed"),
                    LastAttempt = db.QuizAttempts
                        .Where(qa => qa.QuizId == q.Id && qa.StudentId == studentId && qa.Status == "Completed")
                        .OrderByDescending(qa => qa.StartTime)
                        .FirstOrDefault()
                })
                .ToList();

            return View(activeQuizzes);
        }

        // Take a specific quiz
        public ActionResult Take(int id)
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            int studentId = Convert.ToInt32(Session["UserId"]);

            var quiz = db.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefault(q => q.Id == id && q.Status == "Active");

            if (quiz == null)
            {
                TempData["Error"] = "Quiz not found or not available.";
                return RedirectToAction("Index");
            }

            // Check if student has already completed this quiz
            var existingAttempt = db.QuizAttempts
                .FirstOrDefault(qa => qa.QuizId == id && qa.StudentId == studentId && qa.Status == "Completed");

            if (existingAttempt != null)
            {
                TempData["Info"] = "You have already completed this quiz.";
                return RedirectToAction("ViewResult", new { attemptId = existingAttempt.Id });
            }

            // Create new quiz attempt
            var quizAttempt = new QuizAttempt
            {
                QuizId = id,
                StudentId = studentId,
                StartTime = DateTime.Now,
                Status = "InProgress"
            };

            db.QuizAttempts.Add(quizAttempt);
            db.SaveChanges();

            var viewModel = new TakeQuizViewModel
            {
                Quiz = quiz,
                AttemptId = quizAttempt.Id,
                Questions = quiz.Questions.OrderBy(q => q.QuestionOrder).ToList(),
                StartTime = quizAttempt.StartTime
            };

            return View(viewModel);
        }


        [HttpPost]
        public ActionResult SubmitQuiz(int attemptId, FormCollection form)
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            int studentId = Convert.ToInt32(Session["UserId"]);

            var quizAttempt = db.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.Quiz.Questions)
                .FirstOrDefault(qa => qa.Id == attemptId && qa.StudentId == studentId);

            if (quizAttempt == null)
            {
                TempData["Error"] = "Quiz attempt not found.";
                return RedirectToAction("Index");
            }

            // Mark attempt as completed
            quizAttempt.EndTime = DateTime.Now;
            quizAttempt.Status = "Completed";

            int totalScore = 0;
            int maximumScore = quizAttempt.Quiz.Questions.Count();
            bool hasShortAnswers = false;

            // Process each answer
            foreach (var question in quizAttempt.Quiz.Questions)
            {
                string questionKey = "question_" + question.Id;
                string studentAnswer = "";

                // Get the student's answer from the form
                if (form[questionKey] != null)
                {
                    studentAnswer = form[questionKey].ToString().Trim();
                }

                var response = new QuizResponse
                {
                    QuizAttemptId = attemptId,
                    QuestionId = question.Id,
                    StudentAnswer = studentAnswer,
                    ResponseTime = DateTime.Now
                };

                // FIXED: Better logic to determine if it's multiple choice
                bool isMultipleChoice = question.QuestionType == "MultipleChoice" ||
                                       (!string.IsNullOrEmpty(question.OptionA) &&
                                        !string.IsNullOrEmpty(question.OptionB) &&
                                        !string.IsNullOrEmpty(question.OptionC) &&
                                        !string.IsNullOrEmpty(question.OptionD));

                if (isMultipleChoice)
                {
                    // Multiple Choice - Auto grade immediately
                    if (!string.IsNullOrEmpty(studentAnswer) &&
                        !string.IsNullOrEmpty(question.CorrectAnswer) &&
                        studentAnswer.ToUpper() == question.CorrectAnswer.ToUpper())
                    {
                        response.IsCorrect = true;
                        response.PointsAwarded = 1;
                        totalScore++;
                    }
                    else
                    {
                        response.IsCorrect = false;
                        response.PointsAwarded = 0;
                    }
                }
                else
                {
                    // Short answer questions need manual review
                    response.IsCorrect = null;
                    response.PointsAwarded = null;
                    hasShortAnswers = true;
                }

                db.QuizResponses.Add(response);
            }

            // Set grading status based on whether there are actual short answer questions
            if (hasShortAnswers)
            {
                quizAttempt.GradingStatus = "PendingReview";
            }
            else
            {
                quizAttempt.GradingStatus = "AutoGraded";
                quizAttempt.TotalScore = totalScore;
                quizAttempt.MaximumScore = maximumScore;
                quizAttempt.PercentageScore = maximumScore > 0 ? (decimal)totalScore / maximumScore * 100 : 0;
            }

            db.SaveChanges();

            return RedirectToAction("ViewResult", new { attemptId = attemptId });
        }



        // Add this debug action to your QuizController for testing
        [HttpPost]
        public ActionResult DebugSubmitQuiz(int attemptId, FormCollection form)
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            int studentId = Convert.ToInt32(Session["UserId"]);

            var quizAttempt = db.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.Quiz.Questions)
                .FirstOrDefault(qa => qa.Id == attemptId && qa.StudentId == studentId);

            if (quizAttempt == null)
            {
                TempData["Error"] = "Quiz attempt not found.";
                return RedirectToAction("Index");
            }

            // Create a debug view to see what's being submitted
            var debugInfo = new List<string>();
            debugInfo.Add("=== DEBUG INFORMATION ===");
            debugInfo.Add($"Attempt ID: {attemptId}");
            debugInfo.Add($"Student ID: {studentId}");
            debugInfo.Add($"Quiz ID: {quizAttempt.QuizId}");
            debugInfo.Add($"Total Questions: {quizAttempt.Quiz.Questions.Count()}");
            debugInfo.Add("");

            foreach (var question in quizAttempt.Quiz.Questions)
            {
                string questionKey = "question_" + question.Id;
                string studentAnswer = "";

                if (form[questionKey] != null)
                {
                    studentAnswer = form[questionKey].ToString();
                }

                debugInfo.Add($"Question {question.Id} ({question.QuestionType}):");
                debugInfo.Add($"  Question: {question.QuestionText}");
                debugInfo.Add($"  Form Key: {questionKey}");
                debugInfo.Add($"  Student Answer: '{studentAnswer}'");
                debugInfo.Add($"  Correct Answer: '{question.CorrectAnswer}'");
                debugInfo.Add($"  Match: {(studentAnswer.ToUpper().Trim() == question.CorrectAnswer.ToUpper().Trim() ? "YES" : "NO")}");
                debugInfo.Add("");
            }

            debugInfo.Add("=== ALL FORM DATA ===");
            foreach (string key in form.AllKeys)
            {
                debugInfo.Add($"{key}: {form[key]}");
            }

            ViewBag.DebugInfo = debugInfo;
            return View("DebugResult");
        }


        public ActionResult ViewResult(int attemptId)
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            int studentId = Convert.ToInt32(Session["UserId"]);

            var quizAttempt = db.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.Responses)
                .Include(qa => qa.Responses.Select(r => r.Question))
                .FirstOrDefault(qa => qa.Id == attemptId && qa.StudentId == studentId);

            if (quizAttempt == null)
            {
                TempData["Error"] = "Quiz result not found.";
                return RedirectToAction("Index");
            }

            // Check if the quiz attempt is incomplete
            if (quizAttempt.Status != "Completed" || !quizAttempt.EndTime.HasValue)
            {
                // Handle incomplete quiz attempts
                TempData["Info"] = "This quiz was not completed. You can retake it if it's still available.";
                return RedirectToAction("Index");
            }

            return View(quizAttempt);
        }


        // Add this temporary method to QuizController for debugging
        [HttpPost]
        public ActionResult DebugFormData(int attemptId, FormCollection form)
        {
            var debugInfo = new List<string>();
            debugInfo.Add("=== FORM DEBUG INFO ===");
            debugInfo.Add($"Attempt ID: {attemptId}");
            debugInfo.Add($"Total Form Keys: {form.AllKeys.Length}");
            debugInfo.Add("");

            foreach (string key in form.AllKeys)
            {
                debugInfo.Add($"Key: {key} = Value: '{form[key]}'");
            }

            ViewBag.DebugInfo = debugInfo;
            return View("DebugResult");
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