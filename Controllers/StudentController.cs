using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Brain_Mint.Models;

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
        // In StudentController QuizParticipation method, change to:
        public ActionResult QuizParticipation()
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            // Get available quizzes from database
            var quizzes = db.Quizzes.Where(q => q.Status == "Active").ToList();

            ViewBag.Title = "Quiz Participation";
            return View(quizzes);
        }

        // Assignment Participation
        public ActionResult AssignmentParticipation()
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Account");

            // Sample assignment data - replace with database logic
            var assignments = new List<Assignment>
            {
                new Assignment { Id = 1, Title = "Math Homework", DueDate = DateTime.Now.AddDays(3), Status = "Submitted" },
                new Assignment { Id = 2, Title = "Science Report", DueDate = DateTime.Now.AddDays(-1), Status = "Missed" },
                new Assignment { Id = 3, Title = "History Essay", DueDate = DateTime.Now.AddDays(7), Status = "Pending" }
            };

            ViewBag.Title = "Assignment Participation";
            return View(assignments);
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
    }
}