using Brain_Mint.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Brain_Mint.Controllers
{
    public class AccountController : Controller
    {
        private BrainMintDbContext db = new BrainMintDbContext();

        public ActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Signup(User user)
        {
            if (ModelState.IsValid)
            {
                // Check if user already exists
                if (db.Users.Any(u => u.Name == user.Name))
                {
                    ViewBag.Error = "User already exists.";
                    return View();
                }

                // Add user to database
                db.Users.Add(user);
                db.SaveChanges();

                TempData["SignupSuccess"] = "Signup is successful! Please login.";
                return RedirectToAction("Login");
            }

            ViewBag.Error = "All fields are required.";
            return View();
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
            var user = db.Users.FirstOrDefault(u => u.Name == name && u.Role == role);

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

            Session["UserId"] = user.Id;
            Session["UserName"] = user.Name;
            Session["UserEmail"] = user.Email;
            Session["UserRole"] = user.Role;

            TempData["LoginSuccess"] = "Login successful!";

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
}