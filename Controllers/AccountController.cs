using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Brain_Mint.Models;

namespace Brain_Mint.Controllers
{
    public class AccountController : Controller
    {
        // Simulated in-memory user list for testing
        private static List<User> users = new List<User>();

        public ActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Signup(string name, string email, string password, string role)
        {
            if (users.Any(u => u.Name == name))
            {
                ViewBag.Error = "User already exists.";
                return View();
            }

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }

            users.Add(new User
            {
                Name = name,
                Email = email,
                Password = password,
                Role = role
            });

            TempData["SignupSuccess"] = "Signup is successful! Please login.";
            return RedirectToAction("Login");
        }

        public ActionResult Login()
        {
            if (TempData["SignupSuccess"] != null)
            {
                ViewBag.Success = TempData["SignupSuccess"];
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(string name, string password, string role)
        {
            var user = users.FirstOrDefault(u => u.Name == name && u.Role == role);

            if (user == null)
            {
                ViewBag.Error = "User not found.";
                return View();
            }

            if (user.Password != password)
            {
                ViewBag.Error = "Password is wrong.";
                return View();
            }

            Session["UserName"] = user.Name;
            Session["UserEmail"] = user.Email;
            Session["UserRole"] = user.Role;

            TempData["LoginSuccess"] = "Login successful!";;

            // Redirect to appropriate dashboard
            if (user.Role == "Teacher")
                return RedirectToAction("TeacherDashboard", "Teacher");
            else if (user.Role == "Student")
                return RedirectToAction("StudentDashboard", "Student"); 
            else
                return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }
    }

    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}