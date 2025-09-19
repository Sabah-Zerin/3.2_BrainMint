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
                .Include(r => r.QuizAttempt.Responses)
                .Include(r => r.QuizAttempt.Responses.Select(resp => resp.Question))
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

                // Check if all responses are graded
                bool allGraded = attempt.Responses.All(r =>
                    r.PointsAwarded.HasValue ||
                    (r.Question.QuestionType == "MultipleChoice" && r.IsCorrect.HasValue));

                if (allGraded)
                {
                    attempt.GradingStatus = "Graded";

                    // Calculate total score
                    attempt.TotalScore = attempt.Responses
                        .Where(r => r.PointsAwarded.HasValue || r.IsCorrect.HasValue)
                        .Sum(r => r.PointsAwarded ?? (r.IsCorrect == true ? 1 : 0));

                    attempt.MaximumScore = attempt.Responses.Count;
                    attempt.PercentageScore = attempt.MaximumScore > 0 ?
                        (decimal)attempt.TotalScore / attempt.MaximumScore * 100 : 0;
                }
                else
                {
                    attempt.GradingStatus = "PendingReview";
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

        // Replace your existing AssignmentDetails action with this version that handles missing parameters:

        public ActionResult AssignmentDetails(int? id)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            // If no ID provided, redirect to Assignment Participation page
            if (!id.HasValue)
            {
                return RedirectToAction("AssignmentParticipation");
            }

            int teacherId = Convert.ToInt32(Session["UserId"]);

            var assignment = db.Assignments
                .Include(a => a.Submissions)
                .Include(a => a.Submissions.Select(s => s.Student))
                .FirstOrDefault(a => a.Id == id.Value && a.CreatedByUserId == teacherId);

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



        #region Participation

        public ActionResult QuizParticipation()
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            try
            {
                // Get all active quizzes with basic info first
                var allQuizzes = db.Quizzes
                    .Include(q => q.CreatedBy)
                    .Include(q => q.Questions)
                    .Where(q => q.Status == "Active")
                    .OrderByDescending(q => q.CreatedDate)
                    .ToList();

                // Extract quiz IDs as primitive types for Entity Framework
                var quizIds = allQuizzes.Select(q => q.Id).ToList();

                // Get all quiz attempts separately using primitive quiz IDs
                var allAttempts = db.QuizAttempts
                    .Include(qa => qa.Student)
                    .Where(qa => quizIds.Contains(qa.QuizId))
                    .ToList();

                // Create the participation info for active quizzes
                var activeQuizzes = new List<QuizParticipationInfo>();

                foreach (var quiz in allQuizzes)
                {
                    // Get attempts for this specific quiz
                    var quizAttempts = allAttempts.Where(qa => qa.QuizId == quiz.Id).ToList();

                    // Calculate statistics
                    var completedAttempts = quizAttempts.Where(qa => qa.Status == "Completed").ToList();
                    var uniqueParticipants = completedAttempts.Select(qa => qa.StudentId).Distinct().ToList();

                    // Calculate average score from completed attempts with percentage scores
                    var scoresForAverage = completedAttempts
                        .Where(qa => qa.PercentageScore.HasValue)
                        .Select(qa => qa.PercentageScore.Value)
                        .ToList();

                    var quizInfo = new QuizParticipationInfo
                    {
                        Id = quiz.Id,
                        Title = quiz.Title,
                        Description = quiz.Description,
                        Status = quiz.Status,
                        CreatedDate = quiz.CreatedDate,
                        TotalQuestions = quiz.Questions?.Count ?? 0,
                        TimeLimit = quiz.TimeLimit,

                        // Calculate participation statistics using the filtered attempts
                        ParticipantCount = uniqueParticipants.Count,
                        CompletedAttempts = completedAttempts.Count,

                        // Calculate average score
                        AverageScore = scoresForAverage.Any() ? scoresForAverage.Average() : (decimal?)null,

                        // Add teacher name
                        TeacherName = quiz.CreatedBy?.Name ?? "Unknown Teacher"

                        // REMOVED: LastActivity, PendingAttempts - not needed anymore
                    };

                    activeQuizzes.Add(quizInfo);
                }

                // Calculate overall totals using the separate attempts query
                var totalQuizzes = allQuizzes.Count;
                var completedAttemptsForTotal = allAttempts.Where(qa => qa.Status == "Completed").ToList();
                var totalParticipants = completedAttemptsForTotal.Select(qa => qa.StudentId).Distinct().Count();

                // Calculate overall average score
                var allCompletedScores = completedAttemptsForTotal
                    .Where(qa => qa.PercentageScore.HasValue)
                    .Select(qa => qa.PercentageScore.Value)
                    .ToList();

                // Create the view model
                var viewModel = new QuizParticipationViewModel
                {
                    ActiveQuizzes = activeQuizzes,
                    // REMOVED: RecentSessions - not needed anymore
                    TotalQuizzes = totalQuizzes,
                    TotalParticipants = totalParticipants,
                    QuickStats = new QuizQuickStats
                    {
                        TotalQuizzesCreated = totalQuizzes,
                        TotalAttempts = completedAttemptsForTotal.Count,
                        OverallAverageScore = allCompletedScores.Any() ? allCompletedScores.Average() : 0,
                        ActiveQuizzesCount = activeQuizzes.Count,
                        TotalUniqueParticipants = totalParticipants
                    }
                };

                // Pass current teacher's name to the view
                int currentTeacherId = Convert.ToInt32(Session["UserId"]);
                var currentTeacher = db.Users.FirstOrDefault(u => u.Id == currentTeacherId);
                ViewBag.CurrentTeacherName = currentTeacher?.Name ?? "Unknown";

                // Debug information - you can remove this after confirming it works
                ViewBag.DebugInfo = $"Total Quizzes: {totalQuizzes}, Total Attempts: {allAttempts.Count}, " +
                                   $"Completed Attempts: {completedAttemptsForTotal.Count}, " +
                                   $"Total Participants: {totalParticipants}, Quiz IDs: [{string.Join(", ", quizIds)}]";

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the error and return a safe fallback
                ViewBag.Error = "An error occurred while loading quiz participation data: " + ex.Message;

                // Return empty view model to prevent null reference
                var emptyViewModel = new QuizParticipationViewModel
                {
                    ActiveQuizzes = new List<QuizParticipationInfo>(),
                    TotalQuizzes = 0,
                    TotalParticipants = 0,
                    QuickStats = new QuizQuickStats()
                };

                return View(emptyViewModel);
            }
        }



        
        public ActionResult AssignmentParticipation()
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            try
            {
                // Get all active assignments with basic info first
                var allAssignments = db.Assignments
                    .Include(a => a.CreatedBy)
                    .Include(a => a.Submissions)
                    .Include(a => a.Submissions.Select(s => s.Student))
                    .Where(a => a.Status == "Active")
                    .OrderByDescending(a => a.CreatedDate)
                    .ToList();

                // Create the participation info for active assignments
                var activeAssignments = new List<AssignmentParticipationInfo>();

                foreach (var assignment in allAssignments)
                {
                    // Calculate statistics
                    var submissions = assignment.Submissions?.ToList() ?? new List<AssignmentSubmission>();
                    var gradedSubmissions = submissions.Where(s => s.Points.HasValue).ToList();
                    var uniqueParticipants = submissions.Select(s => s.StudentId).Distinct().ToList();

                    // Calculate average score from graded submissions
                    var averageScore = gradedSubmissions.Any()
                        ? gradedSubmissions.Average(s => (decimal)s.Points.Value / assignment.MaxPoints * 100)
                        : (decimal?)null;

                    var assignmentInfo = new AssignmentParticipationInfo
                    {
                        Id = assignment.Id,
                        Title = assignment.Title,
                        Subject = assignment.Subject,
                        DueDate = assignment.DueDate,
                        MaxPoints = assignment.MaxPoints,
                        CreatedDate = assignment.CreatedDate,
                        Status = assignment.Status,

                        // Calculate participation statistics
                        ParticipantCount = uniqueParticipants.Count,
                        TotalSubmissions = submissions.Count,
                        GradedSubmissions = gradedSubmissions.Count,

                        // Calculate average score
                        AverageScore = averageScore,

                        // Add teacher name
                        TeacherName = assignment.CreatedBy?.Name ?? "Unknown Teacher"
                    };

                    activeAssignments.Add(assignmentInfo);
                }

                // Calculate overall totals
                var totalAssignments = allAssignments.Count;
                var allSubmissions = allAssignments.SelectMany(a => a.Submissions ?? new List<AssignmentSubmission>()).ToList();
                var totalParticipants = allSubmissions.Select(s => s.StudentId).Distinct().Count();
                var allGradedSubmissions = allSubmissions.Where(s => s.Points.HasValue).ToList();

                // Calculate overall average score
                var overallAverageScore = allGradedSubmissions.Any()
                    ? allGradedSubmissions.Average(s => {
                        var assignment = allAssignments.First(a => a.Id == s.AssignmentId);
                        return (decimal)s.Points.Value / assignment.MaxPoints * 100;
                    })
                    : 0;

                // Create the view model
                var viewModel = new AssignmentParticipationViewModel
                {
                    ActiveAssignments = activeAssignments,
                    TotalAssignments = totalAssignments,
                    TotalParticipants = totalParticipants,
                    QuickStats = new AssignmentQuickStats
                    {
                        TotalAssignmentsCreated = totalAssignments,
                        TotalSubmissions = allSubmissions.Count,
                        OverallAverageScore = overallAverageScore,
                        ActiveAssignmentsCount = activeAssignments.Count,
                        TotalUniqueParticipants = totalParticipants
                    }
                };

                // Pass current teacher's name to the view
                int currentTeacherId = Convert.ToInt32(Session["UserId"]);
                var currentTeacher = db.Users.FirstOrDefault(u => u.Id == currentTeacherId);
                ViewBag.CurrentTeacherName = currentTeacher?.Name ?? "Unknown";

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the error and return a safe fallback
                ViewBag.Error = "An error occurred while loading assignment participation data: " + ex.Message;

                // Return empty view model to prevent null reference
                var emptyViewModel = new AssignmentParticipationViewModel
                {
                    ActiveAssignments = new List<AssignmentParticipationInfo>(),
                    TotalAssignments = 0,
                    TotalParticipants = 0,
                    QuickStats = new AssignmentQuickStats()
                };

                return View(emptyViewModel);
            }
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