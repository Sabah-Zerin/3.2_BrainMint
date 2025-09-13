using Brain_Mint.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Brain_Mint.Controllers
{
    public class TeacherController : Controller
    {
        private BrainMintDbContext db = new BrainMintDbContext();

        // GET: Teacher/Dashboard
        public ActionResult TeacherDashboard()
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            return View();
        }

        public ActionResult QuizManagement()
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            // Get current teacher's ID
            int teacherId = Convert.ToInt32(Session["UserId"]);

            // Get all quizzes created by this teacher
            var quizzes = db.Quizzes
                .Where(q => q.CreatedByUserId == teacherId)
                .Include(q => q.Questions)
                .OrderByDescending(q => q.CreatedDate)
                .ToList();

            return View(quizzes);
        }

        // Create Quiz GET action
        public ActionResult QuizCreate()
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            return View();
        }


        

        [HttpPost]
        public ActionResult AddQuestion(int quizId, string questionText, string optionA, string optionB,
            string optionC, string optionD, string correctAnswer, int questionOrder, string questionType = "MultipleChoice")
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            int teacherId = Convert.ToInt32(Session["UserId"]);

            var quiz = db.Quizzes.FirstOrDefault(q => q.Id == quizId && q.CreatedByUserId == teacherId);
            if (quiz == null)
            {
                TempData["Error"] = "Quiz not found or you don't have permission to edit it.";
                return RedirectToAction("QuizManagement");
            }

            // Validate input
            if (string.IsNullOrEmpty(questionText))
            {
                ViewBag.Error = "Question text is required.";
                ViewBag.QuizId = quizId;
                ViewBag.QuizTitle = quiz.Title;
                return View();
            }

            // FIXED: Determine question type based on whether all options are provided
            bool hasAllOptions = !string.IsNullOrEmpty(optionA) && !string.IsNullOrEmpty(optionB) &&
                                !string.IsNullOrEmpty(optionC) && !string.IsNullOrEmpty(optionD);

            if (hasAllOptions)
            {
                questionType = "MultipleChoice";

                // Validate multiple choice
                if (string.IsNullOrEmpty(correctAnswer) || !"ABCD".Contains(correctAnswer.ToUpper()))
                {
                    ViewBag.Error = "Please select a valid correct answer (A, B, C, or D).";
                    ViewBag.QuizId = quizId;
                    ViewBag.QuizTitle = quiz.Title;
                    return View();
                }
            }
            else
            {
                questionType = "ShortAnswer";
            }

            try
            {
                var question = new QuizQuestion
                {
                    QuizId = quizId,
                    QuestionText = questionText,
                    QuestionType = questionType,
                    QuestionOrder = questionOrder
                };

                if (questionType == "MultipleChoice")
                {
                    question.OptionA = optionA ?? "";
                    question.OptionB = optionB ?? "";
                    question.OptionC = optionC ?? "";
                    question.OptionD = optionD ?? "";
                    question.CorrectAnswer = correctAnswer?.ToUpper() ?? "";
                }
                else
                {
                    // For short answer questions, clear the options and store the sample answer
                    question.OptionA = "";
                    question.OptionB = "";
                    question.OptionC = "";
                    question.OptionD = "";
                    question.CorrectAnswer = correctAnswer ?? ""; // This will be the sample answer
                }

                db.QuizQuestions.Add(question);
                db.SaveChanges();

                TempData["Success"] = "Question added successfully!";
                return RedirectToAction("QuizEdit", new { id = quizId });
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error adding question: " + ex.Message;
                ViewBag.QuizId = quizId;
                ViewBag.QuizTitle = quiz.Title;
                return View();
            }
        }

        // Edit Question - GET
        public ActionResult EditQuestion(int id)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            int teacherId = Convert.ToInt32(Session["UserId"]);

            var question = db.QuizQuestions
                .Include(q => q.Quiz)
                .FirstOrDefault(q => q.Id == id && q.Quiz.CreatedByUserId == teacherId);

            if (question == null)
            {
                TempData["Error"] = "Question not found or you don't have permission to edit it.";
                return RedirectToAction("QuizManagement");
            }

            return View(question);
        }

        // Edit Question - POST - REPLACE YOUR EXISTING EditQuestion POST METHOD
        [HttpPost]
        public ActionResult EditQuestion(int id, string questionText, string optionA, string optionB,
            string optionC, string optionD, string correctAnswer, int questionOrder)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            int teacherId = Convert.ToInt32(Session["UserId"]);

            var question = db.QuizQuestions
                .Include(q => q.Quiz)
                .FirstOrDefault(q => q.Id == id && q.Quiz.CreatedByUserId == teacherId);

            if (question == null)
            {
                TempData["Error"] = "Question not found or you don't have permission to edit it.";
                return RedirectToAction("QuizManagement");
            }

            // Validate input
            if (string.IsNullOrEmpty(questionText))
            {
                ViewBag.Error = "Question text is required.";
                return View(question);
            }

            // Determine question type based on whether options are provided
            string questionType = "ShortAnswer";
            if (!string.IsNullOrEmpty(optionA) && !string.IsNullOrEmpty(optionB) &&
                !string.IsNullOrEmpty(optionC) && !string.IsNullOrEmpty(optionD))
            {
                questionType = "MultipleChoice";

                // Validate multiple choice
                if (string.IsNullOrEmpty(correctAnswer) || !"ABCD".Contains(correctAnswer.ToUpper()))
                {
                    ViewBag.Error = "Please select a valid correct answer (A, B, C, or D).";
                    return View(question);
                }
            }

            try
            {
                question.QuestionText = questionText;
                question.QuestionType = questionType;
                question.QuestionOrder = questionOrder;

                if (questionType == "MultipleChoice")
                {
                    question.OptionA = optionA ?? "";
                    question.OptionB = optionB ?? "";
                    question.OptionC = optionC ?? "";
                    question.OptionD = optionD ?? "";
                    question.CorrectAnswer = correctAnswer?.ToUpper() ?? "";
                }
                else
                {
                    // For short answer questions
                    question.OptionA = "";
                    question.OptionB = "";
                    question.OptionC = "";
                    question.OptionD = "";
                    question.CorrectAnswer = correctAnswer ?? ""; // This is the sample answer
                }

                db.SaveChanges();

                TempData["Success"] = "Question updated successfully!";
                return RedirectToAction("QuizEdit", new { id = question.QuizId });
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error updating question: " + ex.Message;
                return View(question);
            }
        }

        // Create Quiz POST action - ADD THIS TO YOUR TeacherController
        [HttpPost]
        public ActionResult QuizCreate(string title, string category, string description, string difficulty, int timeLimit)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(category))
            {
                ViewBag.Error = "Title and Category are required.";
                return View();
            }

            try
            {
                // Create new quiz
                var quiz = new Quiz
                {
                    Title = title,
                    Category = category,
                    Description = description ?? "",
                    Difficulty = difficulty ?? "Beginner",
                    TimeLimit = timeLimit > 0 ? timeLimit : 30,
                    CreatedByUserId = Convert.ToInt32(Session["UserId"]),
                    CreatedDate = DateTime.Now,
                    Status = "Draft" // New quizzes start as Draft
                };

                // Save to database
                db.Quizzes.Add(quiz);
                db.SaveChanges();

                TempData["QuizCreateSuccess"] = $"Quiz '{title}' created successfully!";
                return RedirectToAction("QuizManagement");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error creating quiz: " + ex.Message;
                return View();
            }
        }

        // Edit Quiz - shows quiz details and questions - ADD THIS TO YOUR TeacherController
        public ActionResult QuizEdit(int id)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            int teacherId = Convert.ToInt32(Session["UserId"]);

            var quiz = db.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefault(q => q.Id == id && q.CreatedByUserId == teacherId);

            if (quiz == null)
            {
                TempData["Error"] = "Quiz not found or you don't have permission to edit it.";
                return RedirectToAction("QuizManagement");
            }

            return View(quiz);
        }

        // Add Question to Quiz - GET - ADD THIS TO YOUR TeacherController
        public ActionResult AddQuestion(int quizId)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            int teacherId = Convert.ToInt32(Session["UserId"]);

            var quiz = db.Quizzes.FirstOrDefault(q => q.Id == quizId && q.CreatedByUserId == teacherId);
            if (quiz == null)
            {
                TempData["Error"] = "Quiz not found or you don't have permission to edit it.";
                return RedirectToAction("QuizManagement");
            }

            ViewBag.QuizId = quizId;
            ViewBag.QuizTitle = quiz.Title;

            // Get next question order
            var maxOrder = db.QuizQuestions.Where(q => q.QuizId == quizId).Max(q => (int?)q.QuestionOrder) ?? 0;
            ViewBag.NextQuestionOrder = maxOrder + 1;

            return View();
        }

        // Delete Question
        [HttpPost]
        public ActionResult DeleteQuestion(int id)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            int teacherId = Convert.ToInt32(Session["UserId"]);

            var question = db.QuizQuestions
                .Include(q => q.Quiz)
                .FirstOrDefault(q => q.Id == id && q.Quiz.CreatedByUserId == teacherId);

            if (question == null)
            {
                TempData["Error"] = "Question not found or you don't have permission to delete it.";
                return RedirectToAction("QuizManagement");
            }

            try
            {
                int quizId = question.QuizId;
                db.QuizQuestions.Remove(question);
                db.SaveChanges();

                TempData["Success"] = "Question deleted successfully!";
                return RedirectToAction("QuizEdit", new { id = quizId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting question: " + ex.Message;
                return RedirectToAction("QuizEdit", new { id = question.QuizId });
            }
        }

        // Publish Quiz (change status from Draft to Active)
        [HttpPost]
        public ActionResult PublishQuiz(int id)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            int teacherId = Convert.ToInt32(Session["UserId"]);

            var quiz = db.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefault(q => q.Id == id && q.CreatedByUserId == teacherId);

            if (quiz == null)
            {
                TempData["Error"] = "Quiz not found or you don't have permission to edit it.";
                return RedirectToAction("QuizManagement");
            }

            if (!quiz.Questions.Any())
            {
                TempData["Error"] = "Cannot publish quiz without questions. Please add at least one question.";
                return RedirectToAction("QuizEdit", new { id = id });
            }

            try
            {
                quiz.Status = "Active";
                db.SaveChanges();

                TempData["Success"] = "Quiz published successfully! Students can now take this quiz.";
                return RedirectToAction("QuizManagement");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error publishing quiz: " + ex.Message;
                return RedirectToAction("QuizEdit", new { id = id });
            }
        }

        // Add this to your TeacherController
        [HttpPost]
        public ActionResult DeleteQuiz(int id)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            int teacherId = Convert.ToInt32(Session["UserId"]);

            var quiz = db.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefault(q => q.Id == id && q.CreatedByUserId == teacherId);

            if (quiz == null)
            {
                TempData["Error"] = "Quiz not found or you don't have permission to delete it.";
                return RedirectToAction("QuizManagement");
            }

            try
            {
                // Delete all questions first (should cascade automatically, but being explicit)
                var questions = db.QuizQuestions.Where(q => q.QuizId == id).ToList();
                db.QuizQuestions.RemoveRange(questions);

                // Delete the quiz
                db.Quizzes.Remove(quiz);
                db.SaveChanges();

                TempData["Success"] = $"Quiz '{quiz.Title}' and all its questions have been deleted successfully.";
                return RedirectToAction("QuizManagement");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting quiz: " + ex.Message;
                return RedirectToAction("QuizManagement");
            }
        }



        public ActionResult AssignmentManagement()
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");
            return View();
        }

        public ActionResult QuizParticipation()
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");
            return View();
        }

        public ActionResult AssignmentDetails()
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");
            return View();
        }

        public ActionResult ClassPerformance()
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");
            return View();
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