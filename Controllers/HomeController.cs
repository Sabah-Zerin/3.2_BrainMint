/*using System.Web.Mvc;

namespace Brain_Mint.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Session["UserName"] == null)
            {
                // Show Welcome page if not logged in
                return View("Welcome");
            }

            // User is logged in → load Dashboard (Index.cshtml)
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
    }
}
*/

// Controllers/HomeController.cs
using System.Web.Mvc;

namespace Brain_Mint.Controllers
{
    public class HomeController : Controller
    {
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
    }
}