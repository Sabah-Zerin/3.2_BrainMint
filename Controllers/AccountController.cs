using Brain_Mint.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using BCrypt.Net;

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

              
                user.Password = HashPassword(user.Password);

                
                db.Users.Add(user);
                db.SaveChanges();

                TempData["SignupSuccess"] = "Signup is successful! Please login to continue.";
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

            
            if (!IsPasswordValid(password, user.Password))
            {
                ViewBag.Error = "Password is wrong.";
                return View();
            }

          
            if (!IsBCryptHash(user.Password))
            {
                user.Password = HashPassword(password);
                db.SaveChanges();
            }

           
            Session["UserId"] = user.Id;
            Session["UserName"] = user.Name;
            Session["UserEmail"] = user.Email;
            Session["UserRole"] = user.Role;

            
            var loginLog = new LoginLog
            {
                UserId = user.Id,
                UserName = user.Name,
                UserRole = user.Role,
                LoginTime = DateTime.Now,
                IpAddress = Request.UserHostAddress,
                UserAgent = Request.UserAgent
            };

            db.LoginLogs.Add(loginLog);
            db.SaveChanges();

            // Store login log ID in session for logout tracking
            Session["LoginLogId"] = loginLog.Id;

            TempData["LoginSuccess"] = "Login successful!";

           
            if (user.Role == "Teacher")
                return RedirectToAction("TeacherDashboard", "Teacher");
            else if (user.Role == "Student")
                return RedirectToAction("StudentDashboard", "Student");
            else
                return RedirectToAction("Index", "Home");
        }

        public ActionResult LoginActivity()
        {
            
            if (Session["UserRole"]?.ToString() != "Teacher") 
            {
                return RedirectToAction("Login");
            }

            var loginLogs = db.LoginLogs
                .Include(l => l.User)
                .OrderByDescending(l => l.LoginTime)
                .Take(100) 
                .ToList();

            return View(loginLogs);
        }

        
        public ActionResult CurrentlyLoggedIn()
        {
            var currentlyLoggedIn = db.LoginLogs
                .Where(l => l.LogoutTime == null)
                .Include(l => l.User)
                .OrderByDescending(l => l.LoginTime)
                .ToList();

            return View(currentlyLoggedIn);
        }

        // Password handling methods
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
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

        // Check if a string is a BCrypt hash
        private bool IsBCryptHash(string password)
        {
            // BCrypt hashes start with $2a$, $2b$, $2x$, $2y$ and are 60 characters long
            return !string.IsNullOrEmpty(password) &&
                   password.Length == 60 &&
                   (password.StartsWith("$2a$") ||
                    password.StartsWith("$2b$") ||
                    password.StartsWith("$2x$") ||
                    password.StartsWith("$2y$"));
        }

        
        private bool IsPasswordValid(string inputPassword, string storedPassword)
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedPassword))
                return false;

          
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

        public ActionResult Logout()
        {
            
            if (Session["LoginLogId"] != null)
            {
                int loginLogId = Convert.ToInt32(Session["LoginLogId"]);
                var loginLog = db.LoginLogs.Find(loginLogId);
                if (loginLog != null)
                {
                    loginLog.LogoutTime = DateTime.Now;
                    db.SaveChanges();
                }
            }

            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
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