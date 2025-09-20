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

        // FIXED: Single SubmitAssignment method with proper error handling
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitAssignment(SubmitAssignmentViewModel model, HttpPostedFileBase imageFile)
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            int studentId = Convert.ToInt32(Session["UserId"]);

            try
            {
                var assignment = db.Assignments.Find(model.AssignmentId);
                if (assignment == null || assignment.Status != "Active")
                {
                    TempData["Error"] = "Assignment not found or not available.";
                    return RedirectToAction("AssignmentParticipation");
                }

                // Check if both text and image are empty
                if (string.IsNullOrWhiteSpace(model.TextSubmission) &&
                    (imageFile == null || imageFile.ContentLength == 0))
                {
                    TempData["Error"] = "Please provide either text submission or upload an image.";
                    return RedirectToAction("ViewAssignment", new { id = model.AssignmentId });
                }

                // Check if already submitted
                var existingSubmission = db.AssignmentSubmissions
                    .FirstOrDefault(s => s.AssignmentId == model.AssignmentId && s.StudentId == studentId);

                AssignmentSubmission submission;
                if (existingSubmission != null)
                {
                    // Update existing submission
                    submission = existingSubmission;
                    submission.LastModified = DateTime.Now;
                }
                else
                {
                    // Create new submission
                    submission = new AssignmentSubmission
                    {
                        AssignmentId = model.AssignmentId,
                        StudentId = studentId,
                        SubmissionDate = DateTime.Now,
                        Status = DateTime.Now > assignment.DueDate ? "Late" : "Submitted"
                    };
                    db.AssignmentSubmissions.Add(submission);
                }

                // Handle text submission
                if (!string.IsNullOrEmpty(model.TextSubmission))
                {
                    submission.TextSubmission = model.TextSubmission;
                }

                // Handle image upload
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".gif", ".bmp" };
                    var extension = Path.GetExtension(imageFile.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["Error"] = "Only JPG, PNG, PDF, GIF, and BMP files are allowed.";
                        return RedirectToAction("ViewAssignment", new { id = model.AssignmentId });
                    }

                    if (imageFile.ContentLength > 5 * 1024 * 1024) // 5MB limit
                    {
                        TempData["Error"] = "File size must be less than 5MB.";
                        return RedirectToAction("ViewAssignment", new { id = model.AssignmentId });
                    }

                    // Create upload directory if it doesn't exist
                    var uploadsPath = Server.MapPath("~/Uploads/Assignments");
                    if (!Directory.Exists(uploadsPath))
                        Directory.CreateDirectory(uploadsPath);

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
                    var fileName = $"{studentId}_{model.AssignmentId}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";
                    var filePath = Path.Combine(uploadsPath, fileName);
                    imageFile.SaveAs(filePath);

                    submission.ImagePath = "~/Uploads/Assignments/" + fileName;
                    submission.OriginalFileName = imageFile.FileName;
                }

                db.SaveChanges();

                TempData["Success"] = existingSubmission != null ?
                    "Assignment updated successfully!" :
                    "Assignment submitted successfully!";

                return RedirectToAction("AssignmentParticipation");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error submitting assignment: " + ex.Message;
                return RedirectToAction("ViewAssignment", new { id = model.AssignmentId });
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
            //return View(user);
            return View("stu_profile", user);
        }

        // Profile - POST Method (Updated with proper validation and password hashing)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profile(User model, string currentPassword, string newPassword, string confirmPassword)
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            int userId = Convert.ToInt32(Session["UserId"]);
            var user = db.Users.Find(userId);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Profile");
            }

            // Create a clean model state by removing password-related validation errors
            ModelState.Remove("Password");

            // Validate required fields
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "Name is required.");
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Email is required.");
            }
            else if (!IsValidEmail(model.Email))
            {
                ModelState.AddModelError("Email", "Please enter a valid email address.");
            }

            // Check if another user has the same name (excluding current user)
            if (db.Users.Any(u => u.Name == model.Name && u.Id != userId))
            {
                ModelState.AddModelError("Name", "This name is already taken by another user.");
            }

            // Check if another user has the same email (excluding current user)
            if (db.Users.Any(u => u.Email == model.Email && u.Id != userId))
            {
                ModelState.AddModelError("Email", "This email is already registered to another user.");
            }

            // Password change validation
            bool isPasswordChangeRequested = !string.IsNullOrWhiteSpace(newPassword);

            if (isPasswordChangeRequested)
            {
                // Validate current password
                if (string.IsNullOrWhiteSpace(currentPassword))
                {
                    ModelState.AddModelError("", "Current password is required to change password.");
                }
                else if (!IsPasswordValid(currentPassword, user.Password))
                {
                    ModelState.AddModelError("", "Current password is incorrect.");
                }

                // Validate new password
                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    ModelState.AddModelError("", "New password is required.");
                }
                else if (newPassword.Length < 6)
                {
                    ModelState.AddModelError("", "New password must be at least 6 characters long.");
                }

                // Validate confirm password
                if (newPassword != confirmPassword)
                {
                    ModelState.AddModelError("", "New password and confirm password do not match.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update basic information
                    user.Name = model.Name.Trim();
                    user.Email = model.Email.Trim().ToLower();

                    // Update password if requested
                    if (isPasswordChangeRequested)
                    {
                        user.Password = HashPassword(newPassword);
                    }

                    db.SaveChanges();

                    // Update session values
                    Session["UserName"] = user.Name;
                    Session["UserEmail"] = user.Email;

                    TempData["Success"] = "Profile updated successfully!";
                    return RedirectToAction("Profile");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while updating your profile. Please try again.";
                    // Log the exception if you have logging setup
                }
            }

            return View(user);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool IsPasswordValid(string inputPassword, string storedPassword)
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedPassword))
                return false;

            // If stored password is a BCrypt hash, verify using BCrypt
            if (IsBCryptHash(storedPassword))
            {
                return VerifyPassword(inputPassword, storedPassword);
            }
            else
            {
                // If stored password is plain text, compare directly
                return inputPassword == storedPassword;
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }

        private bool IsBCryptHash(string password)
        {
            return !string.IsNullOrEmpty(password) &&
                   password.Length == 60 &&
                   (password.StartsWith("$2a$") ||
                    password.StartsWith("$2b$") ||
                    password.StartsWith("$2x$") ||
                    password.StartsWith("$2y$"));
        }


        // User Management for Students (View Only)
        public ActionResult UserManagement(string search = "", string role = "")
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            var allUsers = db.Users.AsQueryable();

            // Filter by search term (name or email)
            if (!string.IsNullOrEmpty(search))
            {
                allUsers = allUsers.Where(u => u.Name.Contains(search) || u.Email.Contains(search));
            }

            // Filter by role
            if (!string.IsNullOrEmpty(role) && role != "All")
            {
                allUsers = allUsers.Where(u => u.Role == role);
            }

            var users = allUsers.OrderBy(u => u.Role).ThenBy(u => u.Name).ToList();

            // Separate teachers and students
            var teachers = users.Where(u => u.Role == "Teacher").ToList();
            var students = users.Where(u => u.Role == "Student").ToList();

            var viewModel = new UserManagementViewModel
            {
                Teachers = teachers,
                Students = students,
                SearchTerm = search,
                SelectedRole = role,
                TotalTeachers = teachers.Count,
                TotalStudents = students.Count
            };

            return View(viewModel);
        }

        [HttpPost]
        public JsonResult SearchUsers(string search, string role)
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var allUsers = db.Users.AsQueryable();

                // Filter by search term
                if (!string.IsNullOrEmpty(search))
                {
                    allUsers = allUsers.Where(u => u.Name.Contains(search) || u.Email.Contains(search));
                }

                // Filter by role
                if (!string.IsNullOrEmpty(role) && role != "All")
                {
                    allUsers = allUsers.Where(u => u.Role == role);
                }

                var users = allUsers
                    .OrderBy(u => u.Role)
                    .ThenBy(u => u.Name)
                    .Select(u => new { u.Id, u.Name, u.Email, u.Role })
                    .ToList();

                return Json(new { success = true, users = users });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error searching users: " + ex.Message });
            }
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