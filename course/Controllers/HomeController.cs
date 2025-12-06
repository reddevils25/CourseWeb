using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace course.Controllers
{
   
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login", "Account");

            return View();
        }

    }
}
