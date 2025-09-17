using Brain_Mint.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

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

        #region Quiz Management
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

        // Edit Quiz - shows quiz details and questions
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

        // Add Question to Quiz - GET
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

            // Determine question type based on whether all options are provided
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
                    question.OptionA = "";
                    question.OptionB = "";
                    question.OptionC = "";
                    question.OptionD = "";
                    question.CorrectAnswer = correctAnswer ?? "";
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
                    question.OptionA = "";
                    question.OptionB = "";
                    question.OptionC = "";
                    question.OptionD = "";
                    question.CorrectAnswer = correctAnswer ?? "";
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
                var questions = db.QuizQuestions.Where(q => q.QuizId == id).ToList();
                db.QuizQuestions.RemoveRange(questions);
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

        public ActionResult QuizSubmissions(int id)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            int teacherId = Convert.ToInt32(Session["UserId"]);

            var quiz = db.Quizzes
                .Include(q => q.Questions)
                .FirstOrDefault(q => q.Id == id && q.CreatedByUserId == teacherId);

            if (quiz == null)
            {
                TempData["Error"] = "Quiz not found or you don't have permission to view it.";
                return RedirectToAction("QuizManagement");
            }

            // Get all attempts for this quiz
            var attempts = db.QuizAttempts
                .Include(qa => qa.Student)
                .Include(qa => qa.Responses)
                .Include(qa => qa.Responses.Select(r => r.Question))
                .Where(qa => qa.QuizId == id)
                .OrderByDescending(qa => qa.StartTime)
                .ToList();

            ViewBag.Quiz = quiz;
            return View(attempts);
        }
        public ActionResult ReviewQuizAttempt(int id)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            int teacherId = Convert.ToInt32(Session["UserId"]);

            var attempt = db.QuizAttempts
                .Include(qa => qa.Quiz)
                .Include(qa => qa.Student)
                .Include(qa => qa.Responses)
                .Include(qa => qa.Responses.Select(r => r.Question))
                .FirstOrDefault(qa => qa.Id == id && qa.Quiz.CreatedByUserId == teacherId);

            if (attempt == null)
            {
                TempData["Error"] = "Quiz attempt not found or you don't have permission to view it.";
                return RedirectToAction("QuizManagement");
            }

            return View(attempt);
        }

        [HttpPost]
        public ActionResult GradeQuizResponse(int responseId, int? points, string feedback)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            int teacherId = Convert.ToInt32(Session["UserId"]);

            var response = db.QuizResponses
                .Include(r => r.QuizAttempt)
                .Include(r => r.QuizAttempt.Quiz)
                .FirstOrDefault(r => r.Id == responseId && r.QuizAttempt.Quiz.CreatedByUserId == teacherId);

            if (response == null)
            {
                TempData["Error"] = "Response not found.";
                return RedirectToAction("QuizManagement");
            }

            try
            {
                response.PointsAwarded = points;
                response.TeacherFeedback = feedback;
                response.GradedDate = DateTime.Now;
                response.GradedByUserId = teacherId;

                // Update the attempt grading status
                var attempt = response.QuizAttempt;
                if (attempt.Responses.All(r => r.PointsAwarded.HasValue || r.Question.QuestionType != "ShortAnswer"))
                {
                    attempt.GradingStatus = "Graded";

                    // Calculate total score
                    attempt.TotalScore = attempt.Responses
                        .Where(r => r.PointsAwarded.HasValue)
                        .Sum(r => r.PointsAwarded.Value);

                    attempt.MaximumScore = attempt.Responses.Count;
                    attempt.PercentageScore = attempt.MaximumScore > 0 ?
                        (decimal)attempt.TotalScore / attempt.MaximumScore * 100 : 0;
                }

                db.SaveChanges();

                TempData["Success"] = "Response graded successfully!";
                return RedirectToAction("ReviewQuizAttempt", new { id = response.QuizAttemptId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error grading response: " + ex.Message;
                return RedirectToAction("ReviewQuizAttempt", new { id = response.QuizAttemptId });
            }
        }

        #endregion

        #region Assignment Management
        public ActionResult AssignmentManagement()
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            int teacherId = Convert.ToInt32(Session["UserId"]);

            var assignments = db.Assignments
                .Where(a => a.CreatedByUserId == teacherId)
                .Include(a => a.Submissions)
                .OrderByDescending(a => a.CreatedDate)
                .ToList();

            return View(assignments);
        }

        [HttpPost]
        public ActionResult CreateAssignment(CreateAssignmentViewModel model)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields.";
                return RedirectToAction("AssignmentManagement");
            }

            try
            {
                var assignment = new Assignment
                {
                    Title = model.Title,
                    Description = model.Description,
                    Subject = model.Subject,
                    DueDate = model.DueDate,
                    CreatedDate = DateTime.Now,
                    CreatedByUserId = Convert.ToInt32(Session["UserId"]),
                    Status = "Active",
                    MaxPoints = model.MaxPoints,
                    SubmissionType = model.SubmissionType
                };

                db.Assignments.Add(assignment);
                db.SaveChanges();

                TempData["Success"] = $"Assignment '{model.Title}' created successfully!";
                return RedirectToAction("AssignmentManagement");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error creating assignment: " + ex.Message;
                return RedirectToAction("AssignmentManagement");
            }
        }

        public ActionResult AssignmentDetails(int id)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            int teacherId = Convert.ToInt32(Session["UserId"]);

            var assignment = db.Assignments
                .Include(a => a.Submissions)
                .Include(a => a.Submissions.Select(s => s.Student))
                .FirstOrDefault(a => a.Id == id && a.CreatedByUserId == teacherId);

            if (assignment == null)
            {
                TempData["Error"] = "Assignment not found or you don't have permission to view it.";
                return RedirectToAction("AssignmentManagement");
            }

            var submissions = assignment.Submissions.Select(s => new AssignmentSubmissionView
            {
                Id = s.Id,
                StudentName = s.Student.Name,
                StudentEmail = s.Student.Email,
                SubmissionDate = s.SubmissionDate,
                Status = s.Status,
                Points = s.Points,
                HasTextSubmission = !string.IsNullOrEmpty(s.TextSubmission),
                HasImageSubmission = !string.IsNullOrEmpty(s.ImagePath),
                IsLate = s.SubmissionDate > assignment.DueDate
            }).OrderBy(s => s.StudentName).ToList();

            var viewModel = new AssignmentDetailsViewModel
            {
                Assignment = assignment,
                Submissions = submissions,
                TotalSubmissions = submissions.Count,
                PendingGrading = submissions.Count(s => s.Points == null),
                AverageScore = submissions.Where(s => s.Points.HasValue).Any()
                    ? submissions.Where(s => s.Points.HasValue).Average(s => s.Points.Value)
                    : 0
            };

            return View(viewModel);
        }

        // GET: Teacher/EditAssignment/5
        public ActionResult EditAssignment(int id)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            int teacherId = Convert.ToInt32(Session["UserId"]);

            var assignment = db.Assignments.Find(id);
            if (assignment == null || assignment.CreatedByUserId != teacherId)
            {
                TempData["Error"] = "Assignment not found or you don't have permission to edit it.";
                return RedirectToAction("AssignmentManagement");
            }

            var model = new CreateAssignmentViewModel
            {
                Title = assignment.Title,
                Description = assignment.Description,
                Subject = assignment.Subject,
                DueDate = assignment.DueDate,
                MaxPoints = assignment.MaxPoints,
                SubmissionType = assignment.SubmissionType
            };

            ViewBag.AssignmentId = id;
            return View(model);
        }

        // POST: Teacher/EditAssignment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAssignment(int id, CreateAssignmentViewModel model)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            int teacherId = Convert.ToInt32(Session["UserId"]);

            if (ModelState.IsValid)
            {
                try
                {
                    var assignment = db.Assignments.Find(id);
                    if (assignment == null || assignment.CreatedByUserId != teacherId)
                    {
                        TempData["Error"] = "Assignment not found or you don't have permission to edit it.";
                        return RedirectToAction("AssignmentManagement");
                    }

                    assignment.Title = model.Title;
                    assignment.Description = model.Description;
                    assignment.Subject = model.Subject;
                    assignment.DueDate = model.DueDate;
                    assignment.MaxPoints = model.MaxPoints;
                    assignment.SubmissionType = model.SubmissionType;

                    db.SaveChanges();

                    TempData["Success"] = "Assignment updated successfully!";
                    return RedirectToAction("AssignmentDetails", new { id = id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating assignment: " + ex.Message);
                }
            }

            ViewBag.AssignmentId = id;
            return View(model);
        }

        public ActionResult ReviewSubmission(int id)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            int teacherId = Convert.ToInt32(Session["UserId"]);

            var submission = db.AssignmentSubmissions
                .Include(s => s.Assignment)
                .Include(s => s.Student)
                .FirstOrDefault(s => s.Id == id && s.Assignment.CreatedByUserId == teacherId);

            if (submission == null)
            {
                TempData["Error"] = "Submission not found or you don't have permission to review it.";
                return RedirectToAction("AssignmentManagement");
            }

            return View(submission);
        }

        [HttpPost]
        public ActionResult GradeSubmission(int id, int points, string feedback)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            int teacherId = Convert.ToInt32(Session["UserId"]);

            var submission = db.AssignmentSubmissions
                .Include(s => s.Assignment)
                .FirstOrDefault(s => s.Id == id && s.Assignment.CreatedByUserId == teacherId);

            if (submission == null)
            {
                TempData["Error"] = "Submission not found.";
                return RedirectToAction("AssignmentManagement");
            }

            try
            {
                submission.Points = points;
                submission.TeacherFeedback = feedback;
                submission.GradedDate = DateTime.Now;
                submission.GradedByUserId = teacherId;
                submission.Status = "Graded";

                db.SaveChanges();

                TempData["Success"] = "Submission graded successfully!";
                return RedirectToAction("AssignmentDetails", new { id = submission.AssignmentId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error grading submission: " + ex.Message;
                return RedirectToAction("ReviewSubmission", new { id = id });
            }
        }

        [HttpPost]
        public ActionResult DeleteAssignment(int id)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            int teacherId = Convert.ToInt32(Session["UserId"]);

            var assignment = db.Assignments
                .Include(a => a.Submissions)
                .FirstOrDefault(a => a.Id == id && a.CreatedByUserId == teacherId);

            if (assignment == null)
            {
                TempData["Error"] = "Assignment not found or you don't have permission to delete it.";
                return RedirectToAction("AssignmentManagement");
            }

            try
            {
                // Delete associated image files
                foreach (var submission in assignment.Submissions.Where(s => !string.IsNullOrEmpty(s.ImagePath)))
                {
                    var imagePath = Server.MapPath(submission.ImagePath);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                db.Assignments.Remove(assignment);
                db.SaveChanges();

                TempData["Success"] = $"Assignment '{assignment.Title}' deleted successfully.";
                return RedirectToAction("AssignmentManagement");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting assignment: " + ex.Message;
                return RedirectToAction("AssignmentManagement");
            }
        }

        public ActionResult DownloadSubmissionImage(int submissionId)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            int teacherId = Convert.ToInt32(Session["UserId"]);

            var submission = db.AssignmentSubmissions
                .Include(s => s.Assignment)
                .FirstOrDefault(s => s.Id == submissionId && s.Assignment.CreatedByUserId == teacherId);

            if (submission == null || string.IsNullOrEmpty(submission.ImagePath))
            {
                return HttpNotFound();
            }

            var imagePath = Server.MapPath(submission.ImagePath);
            if (!System.IO.File.Exists(imagePath))
            {
                return HttpNotFound();
            }

            var fileName = submission.OriginalFileName ?? "submission_image.jpg";
            return File(imagePath, "application/octet-stream", fileName);
        }
        #endregion

        #region Other Actions
        public ActionResult QuizParticipation()
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
        #endregion

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