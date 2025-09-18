// Controllers/HomeController.cs
using System.Web.Mvc;
using Brain_Mint.Models;

namespace Brain_Mint.Controllers
{
    public class HomeController : Controller
    {
        private BrainMintDbContext db = new BrainMintDbContext();
        public ActionResult Welcome()
        {
            return View();
        }

        public ActionResult Index()
        {
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Welcome");
            }

            if (TempData["LoginSuccess"] != null)
            {
                ViewBag.LoginSuccess = TempData["LoginSuccess"];
            }

            ViewBag.UserName = Session["UserName"]?.ToString();
            ViewBag.UserEmail = Session["UserEmail"]?.ToString();
            ViewBag.UserRole = Session["UserRole"]?.ToString();

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
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