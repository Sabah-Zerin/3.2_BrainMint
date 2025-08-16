using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Brain_Mint.Controllers
{
    public class TeacherController : Controller
    {
        // GET: Teacher/Dashboard
        public ActionResult TeacherDashboard()
        {
            if (Session["UserRole"]?.ToString() != "Teacher")
                return RedirectToAction("Login", "Account");

            return View();
        }

        public ActionResult QuizManagement()
        {
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
    }
}