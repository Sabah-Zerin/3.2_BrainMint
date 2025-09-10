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

        // Create Quiz POST action
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

        // ADD THIS - Edit Quiz action
        public ActionResult QuizEdit(int id)
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            // Sample data - replace with database logic later
            ViewBag.QuizId = id;
            ViewBag.QuizTitle = "Sample Quiz";
            return View();
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