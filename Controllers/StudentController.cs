using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Brain_Mint.Models;
using System.Data.Entity;
using System.IO;

namespace Brain_Mint.Controllers
{
    public class StudentController : Controller
    {
        private BrainMintDbContext db = new BrainMintDbContext();

        public ActionResult StudentDashboard()
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            ViewBag.Title = "Dashboard";
            return View();
        }

        // Quiz Participation
        public ActionResult QuizParticipation()
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            // Redirect to the Quiz controller's Index action
            return RedirectToAction("Index", "Quiz");
        }
        // Assignment Participation
        public ActionResult AssignmentParticipation()
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            int studentId = Convert.ToInt32(Session["UserId"]);

            var assignments = db.Assignments
                .Where(a => a.Status == "Active")
                .Include(a => a.CreatedBy)
                .OrderByDescending(a => a.DueDate)
                .ToList();

            var assignmentViewModels = assignments.Select(a => new AssignmentStudentViewModel
            {
                Assignment = a,
                HasSubmitted = db.AssignmentSubmissions.Any(s => s.AssignmentId == a.Id && s.StudentId == studentId),
                Submission = db.AssignmentSubmissions.FirstOrDefault(s => s.AssignmentId == a.Id && s.StudentId == studentId),
                IsOverdue = DateTime.Now > a.DueDate,
                DaysUntilDue = (a.DueDate - DateTime.Now).Days
            }).ToList();

            return View(assignmentViewModels);
        }

        // Also remove the old duplicate AssignmentParticipation method that returns List<Assignment>

        // View Assignment Details for Student
        public ActionResult ViewAssignment(int id)
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            int studentId = Convert.ToInt32(Session["UserId"]);

            var assignment = db.Assignments
                .Include(a => a.CreatedBy)
                .FirstOrDefault(a => a.Id == id && a.Status == "Active");

            if (assignment == null)
            {
                TempData["Error"] = "Assignment not found or not available.";
                return RedirectToAction("AssignmentParticipation");
            }

            var existingSubmission = db.AssignmentSubmissions
                .FirstOrDefault(s => s.AssignmentId == id && s.StudentId == studentId);

            var viewModel = new SubmitAssignmentViewModel
            {
                AssignmentId = id,
                Assignment = assignment,
                HasExistingSubmission = existingSubmission != null,
                ExistingSubmission = existingSubmission,
                TextSubmission = existingSubmission?.TextSubmission
            };

            return View(viewModel);
        }

        // Submit Assignment
        [HttpPost]
        public ActionResult SubmitAssignment(int assignmentId, string textSubmission, HttpPostedFileBase imageFile)
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            int studentId = Convert.ToInt32(Session["UserId"]);

            var assignment = db.Assignments.Find(assignmentId);
            if (assignment == null || assignment.Status != "Active")
            {
                TempData["Error"] = "Assignment not found or not available.";
                return RedirectToAction("AssignmentParticipation");
            }

            // Check if both text and image are empty
            if (string.IsNullOrWhiteSpace(textSubmission) && imageFile == null)
            {
                TempData["Error"] = "Please provide either text submission or upload an image.";
                return RedirectToAction("ViewAssignment", new { id = assignmentId });
            }

            try
            {
                var existingSubmission = db.AssignmentSubmissions
                    .FirstOrDefault(s => s.AssignmentId == assignmentId && s.StudentId == studentId);

                string imagePath = null;
                string originalFileName = null;

                // Handle image upload
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    var fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        TempData["Error"] = "Please upload a valid image file (JPG, PNG, GIF, BMP).";
                        return RedirectToAction("ViewAssignment", new { id = assignmentId });
                    }

                    // Validate file size (max 5MB)
                    if (imageFile.ContentLength > 5 * 1024 * 1024)
                    {
                        TempData["Error"] = "Image file size should not exceed 5MB.";
                        return RedirectToAction("ViewAssignment", new { id = assignmentId });
                    }

                    // Create upload directory if it doesn't exist
                    var uploadDir = Server.MapPath("~/Uploads/Assignments/");
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    // Generate unique filename
                    var fileName = $"{studentId}_{assignmentId}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                    var filePath = Path.Combine(uploadDir, fileName);

                    // Delete old image if updating submission
                    if (existingSubmission != null && !string.IsNullOrEmpty(existingSubmission.ImagePath))
                    {
                        var oldImagePath = Server.MapPath(existingSubmission.ImagePath);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Save new image
                    imageFile.SaveAs(filePath);
                    imagePath = "~/Uploads/Assignments/" + fileName;
                    originalFileName = imageFile.FileName;
                }

                // Create or update submission
                if (existingSubmission == null)
                {
                    // Create new submission
                    var newSubmission = new AssignmentSubmission
                    {
                        AssignmentId = assignmentId,
                        StudentId = studentId,
                        TextSubmission = textSubmission,
                        ImagePath = imagePath,
                        OriginalFileName = originalFileName,
                        SubmissionDate = DateTime.Now,
                        Status = DateTime.Now > assignment.DueDate ? "Late" : "Submitted"
                    };

                    db.AssignmentSubmissions.Add(newSubmission);
                    TempData["Success"] = "Assignment submitted successfully!";
                }
                else
                {
                    // Update existing submission
                    existingSubmission.TextSubmission = textSubmission;
                    if (!string.IsNullOrEmpty(imagePath))
                    {
                        existingSubmission.ImagePath = imagePath;
                        existingSubmission.OriginalFileName = originalFileName;
                    }
                    existingSubmission.LastModified = DateTime.Now;
                    existingSubmission.Status = DateTime.Now > assignment.DueDate ? "Late" : "Submitted";

                    TempData["Success"] = "Assignment updated successfully!";
                }

                db.SaveChanges();
                return RedirectToAction("AssignmentParticipation");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error submitting assignment: " + ex.Message;
                return RedirectToAction("ViewAssignment", new { id = assignmentId });
            }
        }

        // View Submission Results
        public ActionResult ViewSubmissionResult(int id)
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            int studentId = Convert.ToInt32(Session["UserId"]);

            var submission = db.AssignmentSubmissions
                .Include(s => s.Assignment)
                .Include(s => s.Assignment.CreatedBy)
                .FirstOrDefault(s => s.Id == id && s.StudentId == studentId);

            if (submission == null)
            {
                TempData["Error"] = "Submission not found.";
                return RedirectToAction("AssignmentParticipation");
            }

            return View(submission);
        }

        // Performance
        public ActionResult Performance()
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            // Sample performance data - replace with database logic
            var performance = new StudentPerformance
            {
                QuizScores = new List<ScoreRecord>
                {
                    new ScoreRecord { Name = "Mathematics Quiz", Score = 85 },
                    new ScoreRecord { Name = "Science Quiz", Score = 92 }
                },
                AssignmentScores = new List<ScoreRecord>
                {
                    new ScoreRecord { Name = "Math Homework", Score = 90 },
                    new ScoreRecord { Name = "Science Report", Score = 88 }
                }
            };

            ViewBag.Title = "Performance";
            return View(performance);
        }

        // Profile
        public ActionResult Profile()
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            int userId = Convert.ToInt32(Session["UserId"]);
            var user = db.Users.Find(userId);

            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewBag.Title = "Profile";
            return View(user);
        }

        [HttpPost]
        public ActionResult Profile(User model)
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var user = db.Users.Find(model.Id);
                if (user != null)
                {
                    user.Email = model.Email;
                    // Only update password if provided
                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        user.Password = model.Password;
                    }
                    db.SaveChanges();
                    ViewBag.SuccessMessage = "Profile updated successfully!";
                }
            }

            return View(model);
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